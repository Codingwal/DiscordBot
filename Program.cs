using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;

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
            client.ComponentInteractionCreated += OnComponentInteractionCreated;

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
            slCommands.SlashCommandErrored += OnSlashCommandErrored;

            // Console.WriteLine(client.Intents.HasIntent(DiscordIntents.));

            // Start the bot and run it until the program gets stopped
            await client.ConnectAsync();
            while (true)
            {
                await Task.Delay(data.Config.saveDataFrequency * 1000);
                data.SaveData();
                AdminCommands.UpdateBannedUsers(client);
            }
        }

        private static async Task OnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            string customId = e.Interaction.Data.CustomId;
            if (customId.StartsWith("makesticky"))
            {
                string[] tokens = customId.Split('-');
                ulong channelID = ulong.Parse(tokens[1]);
                ulong msgID = ulong.Parse(tokens[2]);
                data.data.stickyMessages[channelID] = msgID;

                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"{e.User.Username} made a post sticky."));
            }
        }

        private static async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            if (e.Exception is SlashExecutionChecksFailedException ex)
            {
                foreach (var check in ex.FailedChecks)
                {
                    if (check is RequireChannelAttribute)
                        await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().WithContent("This command can only be used in the \"admin-commands\" channel.").AsEphemeral());
                }
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
            {
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

            // Handle sticky messages
            if (data.data.stickyMessages.TryGetValue(e.Channel.Id, out ulong msgId))
            {
                try
                {
                    var msg = await e.Channel.GetMessageAsync(msgId);
                    var embed = new DiscordEmbedBuilder(msg.Embeds[0]);
                    await msg.DeleteAsync();
                    msg = await e.Channel.SendMessageAsync(embed);
                    data.data.stickyMessages[e.Channel.Id] = msg.Id; // Update msgId
                }
                catch (NotFoundException) { } // Message is probably already being moved
            }
        }

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}