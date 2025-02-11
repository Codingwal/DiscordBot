using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace DiscordBot.Commands
{
    public partial class AdminCommands : ApplicationCommandModule
    {
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
                return;
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
    }
}