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
            client.Ready += OnClientReady;
            client.MessageCreated += OnClientMessageCreated;
            client.ModalSubmitted += OnModalSubmitted;

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

            // Start the bot and run it until the program gets stopped
            await client.ConnectAsync();
            while (true)
            {
                await Task.Delay(data.Config.saveDataFrequency * 1000);
                data.SaveData();
            }
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

        private static async Task OnClientMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            // Handle private messages
            if (e.Guild == null)
            {
                await e.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(data.Config.privateMessageResponse));
                return;
            }

            // Handle swear words
            string msg = e.Message.Content.ToLower();
            foreach (var word in data.Config.bannedWords)
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