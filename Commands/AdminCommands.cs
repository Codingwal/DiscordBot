using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot.Commands
{
    public class AdminCommands : BaseCommandModule
    {
        [Command("setfaq")]
        public async Task SetFaq(CommandContext ctx, string faq)
        {
            if (ctx.Channel.Name != "admin-commands")
                return;
            Program.config.config.faq = faq;
            Program.config.SaveConfig();
        }
        [Command("post")]
        public async Task Post(CommandContext ctx, string channel, string msg)
        {
            if (ctx.Channel.Name != "admin-commands")
                return;
            
            
        }
    }
}