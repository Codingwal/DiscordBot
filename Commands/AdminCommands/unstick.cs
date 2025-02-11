using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Commands
{
    public partial class AdminCommands : ApplicationCommandModule
    {
        [SlashCommand("unstick", "Make a currently sticky message an unsticky one")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public static async Task Unstick(InteractionContext ctx)
        {
            Program.data.data.stickyMessages.Remove(ctx.Channel.Id);
            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"Post is no longer sticky").AsEphemeral());
        }
    }
}