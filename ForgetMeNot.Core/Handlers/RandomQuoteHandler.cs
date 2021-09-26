using System.Threading.Tasks;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Services;
using MassTransit;

namespace ForgetMeNot.Core.Handlers
{
    public class RandomQuoteHandler : IConsumer<RandomQuoteRequest>
    {
        private readonly QuoteService _quoteService;

        public RandomQuoteHandler(QuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        public async Task Consume(ConsumeContext<RandomQuoteRequest> context)
        {
            var response = await _quoteService.SearchQuote(context.Message);
            if (response == null)
            {
                await context.RespondAsync(new NotFoundResponse());
            }
            else
            {
                await context.RespondAsync(response);
            }
        }
    }
}
