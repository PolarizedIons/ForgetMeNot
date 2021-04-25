using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ForgetMeNot.Database.Models;
using ForgetMeNot.Services;

namespace ForgetMeNot.Commands
{
    public class Commands : ModuleBase
    {
        private readonly QuoteService _quoteService;
        private readonly GuildSettingsService _guildSettingsService;

        public Commands(QuoteService quoteService, GuildSettingsService guildSettingsService)
        {
            _quoteService = quoteService;
            _guildSettingsService = guildSettingsService;
        }

        [Command("remember")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Remember()
        {
            var message = Context.Message.ReferencedMessage;

            if (message == null)
            {
                var lastMessages = await Context.Channel
                    .GetMessagesAsync(10)
                    .FlattenAsync();
                message = lastMessages
                    ?.Where(x => !x.Author.IsBot)
                    .Skip(1)
                    .FirstOrDefault() as IUserMessage;
            }

            if (message == null)
            {
                await ReplyAsync("What I don't know what message you're talking about :(", messageReference: new MessageReference(Context.Message.Id), allowedMentions:AllowedMentions.None);
                return;
            }

            await _quoteService.RememberQuote(message);
        }

        [Command("forget")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Forget()
        {
            var message = Context.Message.ReferencedMessage;

            if (message == null)
            {
                await ReplyAsync("Sorry, I don't know what you want me to forget!", messageReference: new MessageReference(Context.Message.Id), allowedMentions:AllowedMentions.None);
                return;
            }

            await _quoteService.ForgetQuote(message);
        }

        [Command("forgetmenot emote")]
        [Alias("forgetmenot emoji")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task EmoteSetting(IEmote emote)
        {
            if (!(Context.Channel is IGuildChannel channel))
            {
                await ReplyAsync("Sorry, but you aren't in a guild!", messageReference: new MessageReference(Context.Message.Id), allowedMentions:AllowedMentions.None);
                return;
            }

            await _guildSettingsService.SetSaveReaction(channel.GuildId, emote);
            await ReplyAsync($"Got it! {emote} is your new remember-reaction", messageReference: new MessageReference(Context.Message.Id), allowedMentions:AllowedMentions.None);
        }

        [Command("forgetmenot local")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task LocalQuotes()
        {
            if (!(Context.Channel is IGuildChannel channel))
            {
                await ReplyAsync("Sorry, but you aren't in a guild!", messageReference: new MessageReference(Context.Message.Id), allowedMentions:AllowedMentions.None);
                return;
            }

            await _guildSettingsService.SetLocalQuotes(channel.GuildId, true);
            await ReplyAsync($"Got it! Your quotes are now channel-based", messageReference: new MessageReference(Context.Message.Id), allowedMentions:AllowedMentions.None);
        }

        [Command("forgetmenot global")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GlobalQuotes()
        {
            if (!(Context.Channel is IGuildChannel channel))
            {
                await ReplyAsync("Sorry, but you aren't in a guild!", messageReference: new MessageReference(Context.Message.Id), allowedMentions:AllowedMentions.None);
                return;
            }

            await _guildSettingsService.SetLocalQuotes(channel.GuildId, false);
            await ReplyAsync($"Got it! Your quotes are now server-based", messageReference: new MessageReference(Context.Message.Id), allowedMentions:AllowedMentions.None);

        }

        [Command("quote")]
        public async Task SearchQuoteWithUser(IGuildUser? user = null, [Remainder] string? searchTerm = null)
        {
            var quote = await _quoteService.GetQuote(Context, user, searchTerm);
            await ReplyWithQuote(quote);
        }

        [Command("quote *")]
        public async Task SearchQuoteWithoutUser([Remainder] string? searchTerm = null)
        {
            var quote = await _quoteService.GetQuote(Context, null, searchTerm);
            await ReplyWithQuote(quote);
        }

        private async Task ReplyWithQuote(Quote? quote)
        {
            if (quote == null)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                            .WithDescription("Couldn't find a quote :(")
                            .WithColor(Color.Red)
                            .WithCurrentTimestamp()
                            .Build(),
                        messageReference: new MessageReference(Context.Message.Id),
                        allowedMentions:AllowedMentions.None
                    );
                return;
            }

            var quoteUser = await Context.Guild.GetUserAsync(quote.AuthorId);
            var jumpUrl = $"https://discord.com/channels/{quote.GuildId}/{quote.ChannelId}/{quote.MessageId}";
            var quoteUserName = quoteUser == null ? "Someone" : quoteUser.Nickname ?? $"{quoteUser.Username}#{quoteUser.Discriminator}";
            var quoteMessage = quote.Message.Length > 300 ? quote.Message[..300] + "..." : quote.Message;

            await ReplyAsync(embed: new EmbedBuilder()
                        .AddField($"{quoteUserName} once said...", $"{quoteMessage}\n\n[Jump to Message]({jumpUrl})")
                        .WithThumbnailUrl(quoteUser?.GetAvatarUrl())
                        .WithColor(Color.Blue)
                        .WithTimestamp(quote.CreatedAt)
                        .Build(), 
                    messageReference: new MessageReference(Context.Message.Id),
                    allowedMentions:AllowedMentions.None
                );
        }
    }
}
