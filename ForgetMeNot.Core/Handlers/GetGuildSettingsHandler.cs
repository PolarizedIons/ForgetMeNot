using System.Threading.Tasks;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Transport;
using ForgetMeNot.Core.Services;
using MassTransit;

namespace ForgetMeNot.Core.Handlers
{
    public class GetGuildSettingsHandler : IConsumer<GetGuildSettingRequest>
    {
        private readonly GuildSettingsService _guildSettingsService;

        public GetGuildSettingsHandler(GuildSettingsService guildSettingsService)
        {
            _guildSettingsService = guildSettingsService;
        }

        public async Task Consume(ConsumeContext<GetGuildSettingRequest> context)
        {
            var response = await _guildSettingsService.GetGuildSettings(context.Message.GuildId);
            await context.RespondAsync(response);
        }
    }
}
