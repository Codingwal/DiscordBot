using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace DiscordBot
{
    class Program
    {
        private static Config config;
        private static DiscordClient client;
        private static CommandsNextExtension commands;
        static async Task Main()
        {
            config = Config.GetConfig();

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
            client.MessageCreated += ClientMessageCreated;

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

        private static async Task ClientMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            // Handle swear words
            string msg = e.Message.Content.ToLower();
            foreach (var word in config.bannedWords)
            {
                if (msg.Contains(word))
                {
                    Console.WriteLine($"Detected banned word \"{word}\" used by {e.Author.Username} (Deleted message)");
                    await e.Message.DeleteAsync();
                }
            }
        }

        private static Task ClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}