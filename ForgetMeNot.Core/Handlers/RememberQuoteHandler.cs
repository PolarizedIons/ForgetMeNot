using System.Threading.Tasks;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Services;
using MassTransit;

namespace ForgetMeNot.Core.Handlers
{
    public class RememberQuoteHandler : IConsumer<RememberQuoteRequest>
    {
        private readonly QuoteService _quoteService;

        public RememberQuoteHandler(QuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        public async Task Consume(ConsumeContext<RememberQuoteRequest> context)
        {
            var response = await _quoteService.RememberQuote(context.Message);
            await context.RespondAsync(new HandlerResponse(response));
        }
    }
}
