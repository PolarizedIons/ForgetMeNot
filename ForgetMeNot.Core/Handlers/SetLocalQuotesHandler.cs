using System.Reflection.Metadata;
using System.Threading.Tasks;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Services;
using MassTransit;

namespace ForgetMeNot.Core.Handlers
{
    public class SetLocalQuotesHandler : IConsumer<SetLocalQuotesRequest>
    {
        private readonly GuildSettingsService _guildSettingsService;

        public SetLocalQuotesHandler(GuildSettingsService guildSettingsService)
        {
            _guildSettingsService = guildSettingsService;
        }

        public async Task Consume(ConsumeContext<SetLocalQuotesRequest> context)
        {
            var request = context.Message;
            await _guildSettingsService.SetUsingLocalQuotes(request.GuildId, request.UsingLocal);
            await context.RespondAsync(new HandlerResponse(HandlerResponseCode.Success));
        }
    }
}
