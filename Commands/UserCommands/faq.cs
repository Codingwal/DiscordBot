using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace DiscordBot.Commands
{
    public partial class UserCommands : ApplicationCommandModule
    {
        [SlashCommand("faq", "Prints the up-to-date version of the FAQ")]
        [SlashCooldown(2, 2 * 60, SlashCooldownBucketType.User)]
        public async Task Faq(InteractionContext ctx)
        {
            var msg = new DiscordEmbedBuilder
            {
                Title = "FAQ",
                Description = Program.data.data.faq,
                Color = DiscordColor.Blue,
            };
            await ctx.CreateResponseAsync(msg);
        }
    }
}