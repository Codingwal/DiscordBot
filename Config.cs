using Newtonsoft.Json;

namespace DiscordBot
{
    internal class Config
    {
        public string token;
        public JSONConfig config;
        public static Config GetConfig()
        {
            Config c = new();

            using (StreamReader r = new("config/config.json"))
            {
                string json = r.ReadToEnd();
                c.config = JsonConvert.DeserializeObject<JSONConfig>(json);
            }
            using (StreamReader r = new("config/token.json"))
            {
                string json = r.ReadToEnd();
                c.token = JsonConvert.DeserializeObject<JSONToken>(json).token;
            }

            return c;
        }
        public void SaveConfig()
        {
            using (StreamWriter w = new("config/config.json"))
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                w.Write(json);
            } 
        }
    }

    internal sealed class JSONConfig
    {
        public string prefix;
        public string[] bannedWords;
        public string faq;
    }
    internal sealed class JSONToken
    {
        public string token;
    }
}