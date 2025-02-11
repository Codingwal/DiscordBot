using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DiscordBot
{
    internal class Data
    {
        public string Token { get; private set; }
        public JSONConfig Config { get; set; }
        public JSONData data;
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
            using (StreamReader r = new("data/data.json"))
            {
                string json = r.ReadToEnd();
                c.data = JsonConvert.DeserializeObject<JSONData>(json);
            }
            using (StreamReader r = new("data/users.json"))
            {
                string json = r.ReadToEnd();

                JsonSerializerSettings settings = new();
                settings.Converters.Add(new Utility.JsonPriorityQueueConverter<BannedUserInfo, DateTime>());

                c.Users = JsonConvert.DeserializeObject<JSONUsers>(json, settings);
            }

            return c;
        }
        public void SaveData()
        {
            using (StreamWriter w = new("data/users.json"))
            {
                JsonSerializerSettings settings = new();
                settings.Converters.Add(new Utility.JsonPriorityQueueConverter<BannedUserInfo, DateTime>());

                string json = JsonConvert.SerializeObject(Users, Formatting.Indented, settings);
                w.Write(json);
            }
            using (StreamWriter w = new("data/data.json"))
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                w.Write(json);
            }
        }
    }

    // config.json
    internal sealed class JSONConfig
    {
        public string prefix = "";
        public int saveDataFrequency = 5;
        public List<string> bannedWords = new();
        public string privateMessageResponse = "";
        public string inviteLink = "";
        public ulong defaultRoleId = 0;
    }

    // token.json
    internal sealed class JSONToken
    {
        public string token = "";
    }

    // data.json
    internal sealed class JSONData
    {
        public string faq = "";
        public Dictionary<ulong, ulong> stickyMessages = new(); // <channelID, msgID>
    }

    // users.json
    internal sealed class JSONUsers
    {
        public PriorityQueue<BannedUserInfo, DateTime> bannedUsers = new(); // <(GuildID, UserID, ChannelID, MessageID), time>
        public Dictionary<ulong, JSONUser> users = new();
    }
    public struct BannedUserInfo
    {
        public ulong guildID = 0;
        public ulong userID = 0;
        public ulong channelID = 0;
        public ulong messageID = 0;
        public BannedUserInfo(ulong guildID, ulong userID, ulong channelID, ulong messageID)
        {
            this.guildID = guildID;
            this.userID = userID;
            this.channelID = channelID;
            this.messageID = messageID;
        }
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