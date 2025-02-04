using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace DiscordBot.Commands
{
    public class AdminCommands : ApplicationCommandModule
    {
        // Utility
        private static async Task<DiscordChannel> GetChannelByName(DiscordGuild guild, string name)
        {
            IReadOnlyList<DiscordChannel> channels = await guild.GetChannelsAsync();
            foreach (var channel in channels)
            {
                if (channel.Name == name)
                    return channel;
            }
            return null;
        }

        // setfaq
        [SlashCommand("setfaq", "Update the FAQ")]
        public static async Task SetFaq(InteractionContext ctx)
        {
            if (ctx.Channel.Name != "admin-commands")
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("This command can only be used in the \"admin-commands\" channel.").AsEphemeral());
                return;
            }

            var modal = new DiscordInteractionResponseBuilder()
                .WithTitle("Set the faq")
                .WithCustomId("setfaq")
                .AddComponents(new TextInputComponent("Content", "content"));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }
        public static async Task OnSetFaqConfirmed(ModalSubmitEventArgs e)
        {
            var values = e.Values;
            string content = values["content"];

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{e.Interaction.User.Username} updated the faq."));

            Program.data.Config.faq = content;
        }

        // post
        [SlashCommand("post", "Use this to post a message into any channel")]
        public static async Task Post(InteractionContext ctx)
        {
            if (ctx.Channel.Name != "admin-commands")
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("This command can only be used in the \"admin-commands\" channel.").AsEphemeral());
                return;
            }

            var modal = new DiscordInteractionResponseBuilder()
                .WithTitle("Create a post")
                .WithCustomId("post")
                .AddComponents(new TextInputComponent("Channel name", "channel"))
                .AddComponents(new TextInputComponent("Title", "title"))
                .AddComponents(new TextInputComponent("Content", "content"));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }
        public static async Task OnPostConfirmed(ModalSubmitEventArgs e)
        {
            var values = e.Values;
            string channelName = values["channel"];
            string title = values["title"];
            string content = values["content"];
            var channel = await GetChannelByName(e.Interaction.Guild, channelName);
            if (channel == null)
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Channel could not be found.").AsEphemeral());
                return;
            }

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{e.Interaction.User.Username} posted something in {channelName}."));

            var msg = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Red)
                .WithTitle(title)
                .WithDescription(content);
            await channel.SendMessageAsync(msg);
        }

        [SlashCommand("warn", "Warn a user because of bad behaviour")]
        public static async Task Warn(InteractionContext ctx,
            [Option("User", "The user to warn")] DiscordUser user,
            [Option("Reason", "The reason for the warning")] string reason)
        {
            // Reply to the interaction
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent($"Successfully warned {user.Username}").AsEphemeral());

            // Send private message to user
            var member = (DiscordMember)user;
            var dm = await member.CreateDmChannelAsync();
            await dm.SendMessageAsync(new DiscordEmbedBuilder()
                .WithTitle("Warning")
                .WithDescription($"You have been warned because of {reason}.")
                .WithColor(DiscordColor.Orange));

            // Create warning record
            JSONUserRecord record = new()
            {
                punishmentType = JSONUserRecord.PunishmentType.Warning,
                time = DateTime.Now,
                duration = 0,
                reason = reason,
            };

            // Save last messages of user to warning record
            uint i = 0;
            var messages = await ctx.Channel.GetMessagesAsync(20);
            foreach (var msg in messages)
            {
                if (msg.Author == user)
                {
                    record.lastMessages.Add(msg.Content);
                    await ctx.Channel.DeleteMessageAsync(msg);

                    // Only store up to 10 messages of the warned user
                    i++;
                    if (i > 10)
                        break;
                }
            }

            // Store warning record
            var users = Program.data.Users.users;
            if (users.ContainsKey(user.Id))
                users[user.Id].records.Add(record);
            else
                users.Add(user.Id, new JSONUser() { username = user.Username, records = new() { record } });
        }

        static PriorityQueue<(ulong, ulong), DateTime> bannedUsers = new(); // <(GuildID, UserID), time>
        [SlashCommand("tmpban", "Temporarily ban a user")]
        [SlashRequireBotPermissions(Permissions.BanMembers)]
        public static async Task TmpBan(InteractionContext ctx,
           [Option("User", "The user to ban")] DiscordUser user,
           [Choice("1 minute", 1f / 60f)] [Choice("1 hour", 1)] [Choice("1 day", 24)] [Choice("1 week", 7 * 24)]
           [Option("Duration", "The duration of the ban")] double duration,
           [Option("Reason", "The reason for the temporary ban")] string reason)
        {
            var dm = await ((DiscordMember)user).CreateDmChannelAsync();
            await dm.SendMessageAsync(new DiscordEmbedBuilder()
                .WithTitle("Ban")
                .WithDescription($"You have been banned because of {reason}.\nYou will be unbanned after {duration} hours.")
                .WithColor(DiscordColor.Red));

            try
            {
                await ctx.Guild.BanMemberAsync(user.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"Successfully banned {user.Username} for {duration} hours.").AsEphemeral());

            bannedUsers.Enqueue((ctx.Guild.Id, user.Id), DateTime.Now.AddHours(duration));
            Console.WriteLine($"Banned {user.Username} for {duration} hours.");
        }
        public static async void UpdateBannedUsers(DiscordClient client)
        {
            while (true)
            {
                if (!bannedUsers.TryPeek(out (ulong guildID, ulong userID) pair, out DateTime time)) // Break if there are no more users
                    break;

                if (time - DateTime.Now > TimeSpan.Zero) // Break if there is time left
                {
                    Console.WriteLine(time - DateTime.Now);
                    break;
                }

                bannedUsers.Dequeue();

                // Unban
                DiscordGuild guild = await client.GetGuildAsync(pair.guildID);
                await guild.UnbanMemberAsync(pair.userID);

                DiscordUser user = await client.GetUserAsync(pair.userID);

                var dm = await ((DiscordMember)user).CreateDmChannelAsync();
                await dm.SendMessageAsync(new DiscordEmbedBuilder()
                    .WithTitle("Ban over")
                    .WithDescription($"You are no longer banned.")
                    .WithColor(DiscordColor.Green));

                Console.WriteLine($"Unbanned {user.Username}.");
            }
        }
    }
}