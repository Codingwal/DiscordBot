using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace DiscordBot
{
    class Program
    {
        private static DiscordClient client;
        private static CommandsNextExtension commands;
        static async Task Main(string[] args)
        {
            Config config = Config.GetConfig();

            // Setup client
            DiscordConfiguration discordConfig = new()
            {
                Intents = DiscordIntents.All,
                Token = config.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };
            client = new(discordConfig);
            client.Ready += ClientReady;

            // Setup commands
            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = new string[] { config.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };
            commands = client.UseCommandsNext(commandsConfig);
            commands.RegisterCommands<TestCommands>();

            // Start the bot and run it until the program gets stopped
            await client.ConnectAsync();
            await Task.Delay(-1); // -1 means forever
        }

        private static Task ClientReady(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}