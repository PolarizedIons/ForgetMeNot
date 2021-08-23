using System.Threading.Tasks;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Services;
using MassTransit;

namespace ForgetMeNot.Core.Handlers
{
    public class SetSaveReactionHandler : IConsumer<SetSaveReactionRequest>
    {
        private readonly GuildSettingsService _guildSettingsService;

        public SetSaveReactionHandler(GuildSettingsService guildSettingsService)
        {
            _guildSettingsService = guildSettingsService;
        }

        public async Task Consume(ConsumeContext<SetSaveReactionRequest> context)
        {
            var request = context.Message;
            await _guildSettingsService.SetSaveReaction(request.GuildId, request.Emote);
            await context.RespondAsync(new HandlerResponse(HandlerResponseCode.Success));
        }
    }
}
