using System.Threading.Tasks;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Services;
using MassTransit;

namespace ForgetMeNot.Core.Handlers
{
    public class ForgetQuoteHandler : IConsumer<ForgetQuoteRequest>
    {
        private readonly QuoteService _quoteService;

        public ForgetQuoteHandler(QuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        public async Task Consume(ConsumeContext<ForgetQuoteRequest> context)
        {
            var response = await _quoteService.ForgetQuote(context.Message);
            await context.RespondAsync(new HandlerResponse(response));
        }
    }
}

