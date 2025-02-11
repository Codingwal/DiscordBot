using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace DiscordBot.Commands
{
    public partial class AdminCommands : ApplicationCommandModule
    {
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
    }
}