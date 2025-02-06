using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordBot
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
        public class JsonPriorityQueueConverter<TValue, TPriority> : JsonConverter<PriorityQueue<TValue, TPriority>>
        {
            public override PriorityQueue<TValue, TPriority> ReadJson(JsonReader reader, Type objectType, PriorityQueue<TValue, TPriority> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var list = (List<(TValue, TPriority)>)serializer.Deserialize(reader, typeof(List<(TValue, TPriority)>));
                return new(list);
            }
            public override void WriteJson(JsonWriter writer, PriorityQueue<TValue, TPriority> value, JsonSerializer serializer)
            {
                List<(TValue, TPriority)> list = value.UnorderedItems.ToList();
                serializer.Serialize(writer, list);
            }
        }
    }
}