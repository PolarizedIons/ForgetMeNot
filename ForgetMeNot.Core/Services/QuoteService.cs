using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForgetMeNot.Common;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Extentions;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Database;
using ForgetMeNot.Core.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ForgetMeNot.Core.Services
{
    public class QuoteService : IScopedDiService
    {
        private static readonly Random Random = new Random();
        private readonly DatabaseContext _db;
        private readonly GuildSettingsService _guildSettingsService;

        public QuoteService(DatabaseContext db, GuildSettingsService guildSettingsService)
        {
            _db = db;
            _guildSettingsService = guildSettingsService;
        }

        public async Task<Quote?> FindQuoteByMessageId(ulong messageId)
        {
            return await _db.Quotes.FirstOrDefaultAsync(x => 
                x.MessageId == messageId &&
                x.DeletedAt == null
            );
        }

        public async Task<HandlerResponseCode> RememberQuote(RememberQuoteRequest req)
        {
            var existing = await FindQuoteByMessageId(req.MessageId);
            if (existing != null)
            {
                return HandlerResponseCode.QuoteAlreadySaved;
            }

            var quote = new Quote
            {
                Message = req.Message,
                AuthorId = req.AuthorId,
                ChannelId = req.ChannelId,
                GuildId = req.GuildId,
                MessageId = req.MessageId,
            };
            await _db.AddAsync(quote);
            await _db.SaveChangesAsync();

            return HandlerResponseCode.Success;
        }

        public async Task<HandlerResponseCode> ForgetQuote(ForgetQuoteRequest req)
        {
            var existing = await FindQuoteByMessageId(req.MessageId);
            if (existing == null)
            {
                return HandlerResponseCode.UnknownQuote;
            }

            existing.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return HandlerResponseCode.Success;
        }

        public async Task<Quote?> SearchQuote(RandomQuoteRequest req)
        {
            var localQuotes = await _guildSettingsService.IsUsingLocalQuotes(req.GuildId);

            var query = _db.Quotes
                .Where(
                    x => x.DeletedAt == null && x.GuildId == req.GuildId
                );

            if (localQuotes)
            {
                query = query.Where(x => x.ChannelId == req.ChannelId);
            }

            if (req.UserId != null)
            {
                query = query.Where(x => x.AuthorId == req.UserId);
            }

            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            {
                query = query.Where(x => x.Message.Contains(req.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
            }

            query = query
                .OrderBy(x => x.CreatedAt);

            var count = query.Count();
            if (count == 0)
            {
                return null;
            }

            return query
                .Skip(Random.Next(count))
                .Take(1)
                .FirstOrDefault();
        }

        public IEnumerable<Quote> ListQuotes(ListQuotesRequest filter)
        {
            var query = _db.Quotes
                .Where(
                    x => x.DeletedAt == null &&
                         x.GuildId == filter.GuildId &&
                        x.ChannelId == filter.ChannelId
                );

            query = query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);

            return query.ToList();
        }
    }
}
