using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.DiscordBot.Services;

namespace ForgetMeNot.DiscordBot.Commands
{
    public class QuoteCommands : InteractionModuleBase
    {
        private readonly Random _random = new();
        private readonly QuoteService _quoteService;
        private readonly GuildSettingsService _guildSettingsService;
        private readonly DiscordSocketClient _discord;

        public QuoteCommands(QuoteService quoteService, GuildSettingsService guildSettingsService, DiscordSocketClient discord)
        {
            _quoteService = quoteService;
            _guildSettingsService = guildSettingsService;
            _discord = discord;
        }

        [MessageCommand("Remember Quote")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task RememberQuote(IMessage rawMessage)
        {
            if (rawMessage is not IUserMessage message)
            {
                await Context.Interaction.ModifyOriginalResponseAsync(properties =>
                {
                    properties.AllowedMentions = AllowedMentions.None;
                    properties.Embed = new EmbedBuilder()
                        .WithTitle("I can't remember what bots say!")
                        .WithColor(Color.Red)
                        .Build();
                });
                return;
            }

            var response = await _quoteService.RememberQuote(message);
            var jumpUrl = $"https://discord.com/channels/{((IGuildChannel)message.Channel).GuildId}/{message.Channel.Id}/{message.Id}";
            switch (response)
            {
                case HandlerResponseCode.QuoteAlreadySaved:
                    await Context.Interaction.ModifyOriginalResponseAsync(properties =>
                    {
                        properties.AllowedMentions = AllowedMentions.None;
                        properties.Embed = new EmbedBuilder()
                            .WithTitle("I already know that quote!")
                            .WithDescription($"[Jump to Message]({jumpUrl})")
                            .WithColor(Color.Orange)
                            .Build();
                    });
                    break;
                case HandlerResponseCode.Success:
                {
                    var easterEgg = _random.Next(100) == 0;
                    var text = easterEgg ? $"{_discord.CurrentUser.Username} will remember this...." : "Quote saved!";
                    await Context.Interaction.ModifyOriginalResponseAsync(properties =>
                    {
                        properties.AllowedMentions = AllowedMentions.None;
                        properties.Embed = new EmbedBuilder()
                            .WithTitle(text)
                            .WithDescription($"[Jump to Message]({jumpUrl})")
                            .WithColor(Color.Blue)
                            .Build();
                    });
                    break;
                }
                default:
                    await Context.Interaction.ModifyOriginalResponseAsync(properties =>
                    {
                        properties.AllowedMentions = AllowedMentions.None;
                        properties.Embed = new EmbedBuilder()
                            .WithTitle("Sorry, something went wrong!")
                            .WithColor(Color.Red)
                            .Build();
                    });
                    break;
            }
        }

        [SlashCommand("quote", "Get a random quote")]
        [RequireContext(ContextType.Guild)]
        public async Task SearchQuoteWithUser(IGuildUser? user = null, string? searchTerm = null)
        {
            var quote = await _quoteService.GetQuote(Context, user, searchTerm);
            await ReplyWithQuote(quote);
        }

        private async Task ReplyWithQuote(Quote? quote)
        {
            if (quote == null)
            {
                await Context.Interaction.ModifyOriginalResponseAsync(properties =>
                {
                    properties.AllowedMentions = AllowedMentions.None;
                    properties.Embed = new EmbedBuilder()
                        .WithDescription("Couldn't find a quote :(")
                        .WithColor(Color.Red)
                        .WithCurrentTimestamp()
                        .Build();
                });
                return;
            }

            // Context.Guild is null :(
            var quoteUser = await ((IGuildChannel)Context.Channel).Guild.GetUserAsync(quote.AuthorId);
            var jumpUrl = $"https://discord.com/channels/{quote.GuildId}/{quote.ChannelId}/{quote.MessageId}";
            var quoteUserName = quoteUser == null ? "(Someone)" : quoteUser.Nickname ?? quoteUser.Username;
            var quoteMessage = quote.Message.Length > 300 ? quote.Message[..300] + "..." : quote.Message;

            await Context.Interaction.ModifyOriginalResponseAsync(properties =>
            {
                properties.AllowedMentions = AllowedMentions.None;
                properties.Embed = new EmbedBuilder()
                    .AddField($"{quoteUserName} once said...", $"{quoteMessage}\n\n[Jump to Message]({jumpUrl})")
                    .WithThumbnailUrl(quoteUser?.GetAvatarUrl())
                    .WithColor(Color.Blue)
                    .WithTimestamp(quote.CreatedAt)
                    .Build();
            });
        }
    }
}
