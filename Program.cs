using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;

namespace DiscordBot
{
    class Program
    {
        public static Data data;
        static async Task Main()
        {
            data = Data.GetData();

            // Setup client
            DiscordConfiguration discordConfig = new()
            {
                Intents = DiscordIntents.All,
                Token = data.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };
            DiscordClient client = new(discordConfig);

            // Setup event handlers
            client.Ready += EventHandler.OnClientReady;
            client.MessageCreated += EventHandler.OnClientMessageCreated;
            client.ModalSubmitted += EventHandler.OnModalSubmitted;
            client.ComponentInteractionCreated += EventHandler.OnComponentInteractionCreated;
            client.GuildMemberAdded += EventHandler.OnGuildMemberAdded;

            // Setup commands
            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = new string[] { data.Config.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };
            var commands = client.UseCommandsNext(commandsConfig);

            // Setup slash commands
            var slCommands = client.UseSlashCommands();
            slCommands.RegisterCommands<UserCommands>();
            slCommands.RegisterCommands<AdminCommands>();
            slCommands.SlashCommandErrored += EventHandler.OnSlashCommandErrored;

            // Start the bot and run it until the program gets stopped
            await client.ConnectAsync();
            while (true)
            {
                await Task.Delay(data.Config.saveDataFrequency * 1000);
                data.SaveData();
                AdminCommands.UpdateBannedUsers(client);
            }
        }
    }
}