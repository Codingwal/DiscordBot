using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DiscordBot.Commands
{
    public class UserCommands : BaseCommandModule
    {
        [Command("faq")]
        [Cooldown(2, 120, CooldownBucketType.User)] // 120s = 2min
        public async Task Faq(CommandContext ctx)
        {
            var msg = new DiscordEmbedBuilder
            {
                Title = "FAQ",
                Description = Program.config.config.faq,
                Color = DiscordColor.Blue,
            };
            await ctx.Channel.SendMessageAsync(msg);
        }
    }
}