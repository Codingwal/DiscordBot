using Newtonsoft.Json;

namespace DiscordBot
{
    internal class Data
    {
        public string Token { get; private set; }
        public JSONConfig Config { get; set; }
        public JSONUsers Users { get; set; }

        public static Data GetData()
        {
            Data c = new();

            using (StreamReader r = new("data/config.json"))
            {
                string json = r.ReadToEnd();
                c.Config = JsonConvert.DeserializeObject<JSONConfig>(json);
            }
            using (StreamReader r = new("data/token.json"))
            {
                string json = r.ReadToEnd();
                c.Token = JsonConvert.DeserializeObject<JSONToken>(json).token;
            }
            using (StreamReader r = new("data/users.json"))
            {
                string json = r.ReadToEnd();
                c.Users = JsonConvert.DeserializeObject<JSONUsers>(json);
            }

            return c;
        }
        public void SaveData()
        {
            using (StreamWriter w = new("data/config.json"))
            {
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                w.Write(json);
            }
            using (StreamWriter w = new("data/users.json"))
            {
                string json = JsonConvert.SerializeObject(Users, Formatting.Indented);
                w.Write(json);
            }
        }
    }

    internal sealed class JSONConfig
    {
        public string prefix = "";
        public int saveDataFrequency = 5;
        public List<string> bannedWords = new();
        public string faq = "";
        public string privateMessageResponse = "";
        public string inviteLink = "";
    }

    internal sealed class JSONToken
    {
        public string token = "";
    }

    internal sealed class JSONUsers
    {
        public Dictionary<ulong, JSONUser> users = new();
    }
    public class JSONUser
    {
        public string username = "";
        public List<JSONUserRecord> records = new();
    }
    public class JSONUserRecord
    {
        public enum PunishmentType
        {
            None,
            Warning,
            Timeout,
            Ban
        }
        public PunishmentType punishmentType = PunishmentType.None;
        public DateTime time;
        public long duration = 0;
        public string reason = "";
        public List<string> lastMessages = new();
    }
}