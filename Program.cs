using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace DiscordBot
{
    class Program
    {
        public static Config config;
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
            DiscordClient client = new(discordConfig);

            // Setup event handlers
            client.Ready += OnClientReady;
            client.MessageCreated += OnClientMessageCreated;
            client.ModalSubmitted += OnModalSubmitted;

            // Setup commands
            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = new string[] { config.config.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };
            var commands = client.UseCommandsNext(commandsConfig);
            commands.RegisterCommands<UserCommands>();
            commands.CommandErrored += OnCommandErrored;

            // Setup slash commands
            var slCommands = client.UseSlashCommands();
            slCommands.RegisterCommands<AdminCommands>();

            // Start the bot and run it until the program gets stopped
            await client.ConnectAsync();
            await Task.Delay(-1); // -1 means forever
        }

        private static async Task OnModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
        {
            if (e.Interaction.Type == InteractionType.ModalSubmit)
            {
                switch (e.Interaction.Data.CustomId)
                {
                    case "post":
                        await AdminCommands.OnPostConfirmed(e);
                        break;
                    case "setfaq":
                        await AdminCommands.OnSetFaqConfirmed(e);
                        break;
                    default:
                        throw new();
                }
            }
        }

        private static async Task OnCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
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

        private static async Task OnClientMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            // Handle private messages
            if (e.Guild == null)
            {
                await e.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(config.config.privateMessageResponse));
                return;
            }

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

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}