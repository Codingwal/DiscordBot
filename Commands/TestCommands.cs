using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DiscordBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("test")]
        public async Task Test(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Hello {ctx.User.Username}!");
        }
        [Command("add")]
        public async Task Add(CommandContext ctx, int a, int b)
        {
            await ctx.Channel.SendMessageAsync($"{a + b}");
        }

        [Command("embed")]
        public async Task Embed(CommandContext ctx)
        {
            // var msg = new DiscordMessageBuilder()
            //     .AddEmbed(new DiscordEmbedBuilder()
            //         .WithTitle("This is a discord embed")
            //         .WithDescription($"This command was executed by {ctx.User.Username}")
            //         .WithColor(DiscordColor.Blue));

            var msg = new DiscordEmbedBuilder
            {
                Title = "This is a discord embed",
                Description = $"This command was executed by {ctx.User.Username}",
                Color = DiscordColor.Blue,
            };

            await ctx.Channel.SendMessageAsync(msg);
        }
    }
}