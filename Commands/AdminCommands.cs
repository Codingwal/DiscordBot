using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Commands
{
    public class AdminCommands : ApplicationCommandModule
    {
        // Utility
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

        // setfaq
        [SlashCommand("setfaq", "Update the FAQ")]
        public static async Task SetFaq(InteractionContext ctx)
        {
            if (ctx.Channel.Name != "admin-commands")
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("This command can only be used in the \"admin-commands\" channel.").AsEphemeral());
                return;
            }

            var modal = new DiscordInteractionResponseBuilder()
                .WithTitle("Set the faq")
                .WithCustomId("setfaq")
                .AddComponents(new TextInputComponent("Content", "content"));

            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }
        public static async Task OnSetFaqConfirmed(ModalSubmitEventArgs e)
        {
            var values = e.Values;
            string content = values["content"];

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{e.Interaction.User.Username} updated the faq."));

            Program.config.config.faq = content;
            Program.config.SaveConfig();
        }

        // post
        [SlashCommand("post", "Use this to post a message into any channel")]
        public static async Task Post(InteractionContext ctx)
        {
            if (ctx.Channel.Name != "admin-commands")
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("This command can only be used in the \"admin-commands\" channel.").AsEphemeral());
                return;
            }

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
            var channel = await GetChannelByName(e.Interaction.Guild, channelName);
            if (channel == null)
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Channel could not be found.").AsEphemeral());
                return;
            }

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{e.Interaction.User.Username} posted something in {channelName}."));

            var msg = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Red)
                .WithTitle(title)
                .WithDescription(content);
            await channel.SendMessageAsync(msg);
        }
    }
}