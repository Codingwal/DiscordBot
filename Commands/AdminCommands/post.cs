using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Commands
{
    public partial class AdminCommands : ApplicationCommandModule
    {
        [SlashCommand("post", "Use this to post a message into any channel")]
        [RequireChannel("admin-commands")]
        public static async Task Post(InteractionContext ctx)
        {
            var modal = new DiscordInteractionResponseBuilder()
                .WithTitle("Create a post")
                .WithCustomId("post")
                .AddComponents(new TextInputComponent("Channel name", "channel"))
                .AddComponents(new TextInputComponent("Title", "title"))
                .AddComponents(new TextInputComponent("Content", "content"));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }
        public static async Task OnPostConfirmed(ModalSubmitEventArgs e)
        {
            var values = e.Values;
            string channelName = values["channel"];
            string title = values["title"];
            string content = values["content"];
            var channel = await Utility.GetChannelAsync(e.Interaction.Guild, channelName);
            if (channel == null)
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Channel could not be found.").AsEphemeral());
                return;
            }

            // Send post
            var msg = await channel.SendMessageAsync(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Red)
                .WithTitle(title)
                .WithDescription(content));

            // Reply to command (with "Make post sticky" button)
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Successfully created post.")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, $"makesticky-{channel.Id}-{msg.Id}", "Make post sticky"))
                .AsEphemeral());

            // Notify all others in the admin-commands channel
            await e.Interaction.Channel.SendMessageAsync(new DiscordMessageBuilder()
                .WithContent($"{e.Interaction.User.Username} posted something in {channelName}."));
        }
    }
}