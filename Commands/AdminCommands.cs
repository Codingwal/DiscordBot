using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Commands
{
    public class AdminCommands : ApplicationCommandModule
    {
        private static async Task<DiscordChannel> GetChannelByName(DiscordGuild guild, string name)
        {
            IReadOnlyList<DiscordChannel> channels = await guild.GetChannelsAsync();
            foreach (var channel in channels)
            {
                if (channel.Name == name)
                    return channel;
            }
            return null;
        }
        // [Command("setfaq")]
        // public async Task SetFaq(CommandContext ctx, string faq)
        // {
        //     if (ctx.Channel.Name != "admin-commands")
        //         return;

        //     DiscordButtonComponent button = new(DSharpPlus.ButtonStyle.Danger, "setfaq_cancel", "Cancel");
        //     var msg = new DiscordMessageBuilder()
        //         .AddEmbed(new DiscordEmbedBuilder()
        //             .WithColor(DiscordColor.Gray)
        //             .WithTitle("Waiting for message"))
        //         .AddComponents(button);

        //     Program.config.config.faq = faq;
        //     Program.config.SaveConfig();
        // }
        [SlashCommand("post", "Use this to post a message into any channel")]
        public static async Task Post(InteractionContext ctx)
        {
            if (ctx.Channel.Name != "admin-commands")
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                    new DiscordInteractionResponseBuilder().WithContent("Dieser Befehl kann nur im Kanal \"admin-commands\" verwendet werden."));
                return;
            }

            var modal = new DiscordInteractionResponseBuilder()
                .WithTitle("Create a post")
                .WithCustomId("post-modal")
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
            var channel = await GetChannelByName(e.Interaction.Guild, channelName);
            if (channel == null)
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Channel konnte nicht gefunden werden.").AsEphemeral());
                return;
            }

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{((DiscordMember)e.Interaction.User).Nickname} hat etwas in {channelName} gepostet."));

            var msg = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Red)
                .WithTitle(title)
                .WithDescription(content);
            await channel.SendMessageAsync(msg);
        }
    }
}