using Newtonsoft.Json;

namespace DiscordBot
{
    internal class Config
    {
        public string token;
        public string prefix;
        public string[] bannedWords;
        public static Config GetConfig()
        {
            Config config = new();

            using (StreamReader sr = new("config/config.json"))
            {
                string json = sr.ReadToEnd();
                JSONConfig data = JsonConvert.DeserializeObject<JSONConfig>(json);
                config.prefix = data.prefix;
                config.bannedWords = data.bannedWords;
            }
            using (StreamReader sr = new("config/token.json"))
            {
                string json = sr.ReadToEnd();
                JSONToken data = JsonConvert.DeserializeObject<JSONToken>(json);
                config.token = data.token;
            }

            return config;
        }
    }

    internal sealed class JSONConfig
    {
        public string prefix;
        public string[] bannedWords;
    }
    internal sealed class JSONToken
    {
        public string token;
    }
}