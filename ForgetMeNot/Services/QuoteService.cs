using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ForgetMeNot.Database;
using ForgetMeNot.Database.Models;
using ForgetMeNot.Extentions;

namespace ForgetMeNot.Services
{
    public class QuoteService : IScopedDiService
    {
        private static readonly Random Random = new Random();
        private readonly DatabaseContext _db;
        private readonly IDiscordClient _client;
        private readonly GuildSettingsService _guildSettingsService;

        public QuoteService(DatabaseContext db, DiscordSocketClient client, GuildSettingsService guildSettingsService)
        {
            _db = db;
            _client = client;
            _guildSettingsService = guildSettingsService;
        }

        public async Task<Quote?> FindQuoteByMessageId(ulong messageId)
        {
            return await AsyncEnumerable.FirstOrDefaultAsync(_db.Quotes, x => 
                    x.MessageId == messageId &&
                    x.DeletedAt == null
                );
        }

        public async Task<bool> RememberQuote(IUserMessage msg)
        {
            var existing = await FindQuoteByMessageId(msg.Id);
            if (existing != null)
            {
                await msg.Channel.SendMessageAsync("I already remembered that!", messageReference: new MessageReference(msg.Id), allowedMentions:AllowedMentions.None);
                return false;
            }

            if (!(msg.Channel is IGuildChannel channel))
            {
                throw new Exception("The quote you are trying to save is not part of any guild.");
            }

            var quote = new Quote
            {
                Message = msg.Content,
                AuthorId = msg.Author.Id,
                ChannelId = channel.Id,
                GuildId = channel.GuildId,
                MessageId = msg.Id,
            };
            await _db.AddAsync(quote);
            await _db.SaveChangesAsync();

            var easterEgg = Random.Next(100) == 0; 
            await msg.Channel.SendMessageAsync(easterEgg ? $"{_client.CurrentUser.Username} will remember this...." : "Quote saved!", messageReference: new MessageReference(msg.Id), allowedMentions:AllowedMentions.None);

            return true;
        }

        public async Task<bool> ForgetQuote(IUserMessage msg)
        {
            var existing = await FindQuoteByMessageId(msg.Id);
            if (existing == null)
            {
                await msg.Channel.SendMessageAsync("I don't remember that quote!", messageReference: new MessageReference(msg.Id), allowedMentions:AllowedMentions.None);
                return false;
            }

            existing.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await msg.Channel.SendMessageAsync("I have forgotten that quote!", messageReference: new MessageReference(msg.Id), allowedMentions:AllowedMentions.None);

            return true;
        }

        public async Task<Quote?> GetQuote(ICommandContext context, IGuildUser? user, string? searchTerm)
        {
            var localQuotes = await _guildSettingsService.IsLocalQuotes(context.Guild.Id);

            var query = _db.Quotes
                .AsQueryable()
                .Where(
                    x => x.DeletedAt == null && x.GuildId == context.Guild.Id
                );

            if (localQuotes)
            {
                query = query.Where(x => x.ChannelId == context.Channel.Id);
            }

            if (user != null)
            {
                query = query.Where(x => x.AuthorId == user.Id);
            }

            if (searchTerm != null)
            {
                query = query.Where(x => x.Message.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));
            }

            var count = query.Count();
            
            return query
                .Skip(Random.Next(count))
                .Take(1)
                .FirstOrDefault();
        }
    }
}
