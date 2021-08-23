using System.Threading.Tasks;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Services;
using MassTransit;

namespace ForgetMeNot.Core.Handlers
{
    public class SearchQuoteHandler : IConsumer<SearchQuoteRequest>
    {
        private readonly QuoteService _quoteService;

        public SearchQuoteHandler(QuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        public async Task Consume(ConsumeContext<SearchQuoteRequest> context)
        {
            var response = await _quoteService.SearchQuote(context.Message);
            if (response == null)
            {
                await context.RespondAsync(new QuoteNotFoundResponse());
            }
            else
            {
                await context.RespondAsync(response);
            }
        }
    }
}
