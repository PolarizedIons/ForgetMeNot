using System;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Transport;

namespace ForgetMeNot.Api.Models
{
    public class QuoteResponse
    {
        private readonly Quote _quote;

        public QuoteResponse(Quote quote, DiscordUser user)
        {
            _quote = quote;
            Author = user;
        }

        public Guid Id => _quote.Id;
        public DateTime CreatedAt => _quote.CreatedAt;
        public string Quote => _quote.Message;

        public DiscordUser Author { get; }
    }
}
