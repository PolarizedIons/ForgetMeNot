using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
                await ReplyAsync("What I don't know what message you're talking about :(");
                return;
            }

            await _quoteService.RememberQuote(message);
        }

        [Command("emote")]
        [Alias("emoji")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task EmoteSetting(IEmote emote)
        {
            if (!(Context.Channel is IGuildChannel channel))
            {
                await ReplyAsync("Sorry, but you aren't in a guild!");
                return;
            }

            await _guildSettingsService.SetSaveReaction(channel.GuildId, emote);
            await ReplyAsync($"Got it! {emote} is your new remember-reaction");
        }

        [Command("quote")]
        public async Task DisplayQuote(IGuildUser? user = null)
        {
            var quote = _quoteService.GetQuote(Context.Guild.Id, user);

            if (quote == null)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithDescription("Couldn't find a quote :(")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build()
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
                .Build()
            );
        }
    }
}
