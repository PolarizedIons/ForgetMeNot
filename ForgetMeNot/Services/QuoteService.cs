using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
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

        public QuoteService(DatabaseContext db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        public async Task<Quote?> FindQuoteByMessageId(ulong messageId)
        {
            return await _db.Quotes
                .FirstOrDefaultAsync(x => 
                    x.MessageId == messageId &&
                    x.DeletedAt == null
                );
        }

        public async Task<bool> RememberQuote(IUserMessage msg)
        {
            var existing = await FindQuoteByMessageId(msg.Id);
            if (existing != null)
            {
                await msg.Channel.SendMessageAsync("I already remembered that!", messageReference: new MessageReference(msg.Id));
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
            await msg.Channel.SendMessageAsync(easterEgg ? $"{_client.CurrentUser.Username} will remember this...." : "Quote saved!", messageReference: new MessageReference(msg.Id));

            return true;
        }

        public Quote? GetQuote(ulong guildId, IGuildUser? user)
        {
            var query = _db.Quotes
                .AsQueryable()
                .Where(
                    x => x.DeletedAt == null && x.GuildId == guildId
                );

            if (user != null)
            {
                query = query.Where(x => x.AuthorId == user.Id);
            }

            var count = query.Count();
            
            return query
                .Skip(Random.Next(count))
                .Take(1)
                .FirstOrDefault();
        }
    }
}
