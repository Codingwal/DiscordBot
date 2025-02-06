using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace DiscordBot.Commands
{
    public class AdminCommands : ApplicationCommandModule
    {
        // setfaq
        [SlashCommand("setfaq", "Update the FAQ")]
        [RequireChannel("admin-commands")]
        public static async Task SetFaq(InteractionContext ctx)
        {
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
        [RequireChannel("admin-commands")]
        public static async Task Post(InteractionContext ctx)
        {
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
            var channel = await Utility.GetChannelAsync(e.Interaction.Guild, channelName);
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
        [SlashRequireUserPermissions(Permissions.KickMembers)]
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

        [SlashCommand("tmpban", "Temporarily ban a user")]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        public static async Task TmpBan(InteractionContext ctx,
           [Option("User", "The user to ban")] DiscordUser user,
           [Choice("1 hour", 1)] [Choice("1 day", 24)] [Choice("1 week", 7 * 24)]
           [Option("Duration", "The duration of the ban")] double duration,
           [Option("Reason", "The reason for the temporary ban")] string reason)
        {
            // Inform user that they have been banned
            var dm = await ((DiscordMember)user).CreateDmChannelAsync();
            var msg = await dm.SendMessageAsync(new DiscordEmbedBuilder()
                .WithTitle("Ban")
                .WithDescription($"You have been banned because of {reason}.\nYou will be unbanned after {duration} hours.")
                .WithColor(DiscordColor.Red));

            // Ban the user
            try
            {
                await ctx.Guild.BanMemberAsync(user.Id);
            }
            catch (UnauthorizedException)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Failed to ban {user.Username} as he is the guild owner.").AsEphemeral());
            }

            // Inform the banner that the ban has been successfull
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"Successfully banned {user.Username} for {duration} hours.").AsEphemeral());

            Program.data.Users.bannedUsers.Enqueue(new(ctx.Guild.Id, user.Id, dm.Id, msg.Id), DateTime.Now.AddHours(duration));
            Console.WriteLine($"Banned {user.Username} for {duration} hours.");
        }
        public static async void UpdateBannedUsers(DiscordClient client)
        {
            while (true)
            {
                if (!Program.data.Users.bannedUsers.TryPeek(out var info, out DateTime time)) // Break if there are no more users
                    break;

                if (time - DateTime.Now > TimeSpan.Zero) // Break if there is time left
                    break;

                Program.data.Users.bannedUsers.Dequeue();

                // Unban
                DiscordGuild guild = await client.GetGuildAsync(info.guildID);
                await guild.UnbanMemberAsync(info.userID);

                // Log
                DiscordUser user = await client.GetUserAsync(info.userID);
                Console.WriteLine($"Unbanned {user.Username}.");

                // Notify the user that he has been unbanned
                var dm = await client.GetChannelAsync(info.channelID);
                var msg = await dm.GetMessageAsync(info.messageID);
                await msg.ModifyAsync(new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Unban")
                        .WithDescription($"You have been unbanned.\n{Program.data.Config.inviteLink}")
                        .WithColor(DiscordColor.Green)));
            }
        }

        [SlashCommand("unban", "Unban a user")]
        [RequireUserPermissions(Permissions.BanMembers)]
        public static async void UnbanUser(InteractionContext ctx, [Option("user", "The user to unban")] DiscordUser user)
        {
            try
            {
                await ctx.Guild.UnbanMemberAsync(user);
            }
            catch (Exception)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Failed to unban {user.Username}.").AsEphemeral());
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"Unbanned {user.Username}.").AsEphemeral());
        }
    }
}