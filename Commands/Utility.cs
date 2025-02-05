using DSharpPlus.Entities;

namespace DiscordBot.Commands
{
    public static class Utility
    {
        public static async Task<DiscordChannel> GetChannelAsync(DiscordGuild guild, string name)
        {
            IReadOnlyList<DiscordChannel> channels = await guild.GetChannelsAsync();
            foreach (var channel in channels)
            {
                if (channel.Name == name)
                    return channel;
            }
            return null;
        }
    }
}