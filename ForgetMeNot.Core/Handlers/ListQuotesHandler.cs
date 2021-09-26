using System.Threading.Tasks;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Services;
using MassTransit;

namespace ForgetMeNot.Core.Handlers
{
    public class ListQuotesHandler : IConsumer<ListQuotesRequest>
    {
        private readonly QuoteService _quoteService;

        public ListQuotesHandler(QuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        public async Task Consume(ConsumeContext<ListQuotesRequest> context)
        {
            var filter = context.Message;
            var quotes = _quoteService.ListQuotes(filter);
            await context.RespondAsync(new ListResponse<Quote>(quotes));
        }
    }
}