using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Commands
{
    public partial class AdminCommands : ApplicationCommandModule
    {
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