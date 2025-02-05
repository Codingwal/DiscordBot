using DSharpPlus.SlashCommands;

namespace DiscordBot.Commands
{
    public class RequireChannelAttribute : SlashCheckBaseAttribute
    {
        private string channelName;
        public RequireChannelAttribute(string channelName)
        {
            this.channelName = channelName;
        }
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            var channel = await Utility.GetChannelAsync(ctx.Guild, channelName);
            return ctx.Channel == channel;
        }
    }
}