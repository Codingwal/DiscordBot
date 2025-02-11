using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;

namespace DiscordBot
{
    public class EventHandler
    {
        public static async Task OnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            string customId = e.Interaction.Data.CustomId;
            if (customId.StartsWith("makesticky"))
            {
                string[] tokens = customId.Split('-');
                ulong channelID = ulong.Parse(tokens[1]);
                ulong msgID = ulong.Parse(tokens[2]);
                Program.data.data.stickyMessages[channelID] = msgID;

                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"{e.User.Username} made a post sticky."));
            }
        }

        public static async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
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

        public static async Task OnModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
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

        public static async Task OnClientMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            // Handle private messages
            if (e.Guild == null)
            {
                await e.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(Program.data.Config.privateMessageResponse));
                return;
            }

            // Handle swear words
            {
                string msg = e.Message.Content.ToLower();
                foreach (var word in Program.data.Config.bannedWords)
                {
                    if (msg.Contains(word))
                    {
                        Console.WriteLine($"Detected banned word \"{word}\" used by {e.Author.Username} (Deleted message)");
                        await e.Message.DeleteAsync();
                    }
                }
            }

            // Handle sticky messages
            if (Program.data.data.stickyMessages.TryGetValue(e.Channel.Id, out ulong msgId))
            {
                try
                {
                    var msg = await e.Channel.GetMessageAsync(msgId);
                    var embed = new DiscordEmbedBuilder(msg.Embeds[0]);
                    await msg.DeleteAsync();
                    msg = await e.Channel.SendMessageAsync(embed);
                    Program.data.data.stickyMessages[e.Channel.Id] = msg.Id; // Update msgId
                }
                catch (NotFoundException) { } // Message is probably already being moved
            }
        }

        public static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}