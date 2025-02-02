using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace DiscordBot
{
    class Program
    {
        public static Config config;
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
                StringPrefixes = new string[] { config.config.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };
            commands = client.UseCommandsNext(commandsConfig);
            commands.RegisterCommands<UserCommands>();
            commands.RegisterCommands<AdminCommands>();

            commands.CommandErrored += CommandErrored;

            // Start the bot and run it until the program gets stopped
            await client.ConnectAsync();
            await Task.Delay(-1); // -1 means forever
        }

        private static async Task CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Exception is ChecksFailedException exception)
            {
                string timeLeft = "";
                foreach (var check in exception.FailedChecks)
                {
                    var cooldown = (CooldownAttribute)check;
                    timeLeft = cooldown.GetRemainingCooldown(e.Context).ToString(@"hh\:mm\:ss");
                }
                var msg = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Please wait for the cooldown to end",
                    Description = $"Time: {timeLeft}"
                };
                await e.Context.Channel.SendMessageAsync(msg);
            }
        }

        private static async Task ClientMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            // Handle swear words
            string msg = e.Message.Content.ToLower();
            foreach (var word in config.config.bannedWords)
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