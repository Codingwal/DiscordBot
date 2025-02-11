using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Commands
{
    public partial class AdminCommands : ApplicationCommandModule
    {
        [SlashCommand("setfaq", "Update the FAQ")]
        [RequireChannel("admin-commands")]
        public static async Task SetFaq(InteractionContext ctx)
        {
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

            Program.data.data.faq = content;
        }
    }
}