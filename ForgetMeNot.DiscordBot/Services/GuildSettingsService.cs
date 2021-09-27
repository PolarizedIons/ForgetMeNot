using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;
using ForgetMeNot.Common;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Extentions;
using ForgetMeNot.Common.Transport;
using MassTransit;

namespace ForgetMeNot.DiscordBot.Services
{
    public class GuildSettingsService : ISingletonDiService
    {
        private readonly IRequestClient<SetSaveReactionRequest> _setSaveReactionClient;
        private readonly IRequestClient<SetLocalQuotesRequest> _setUsingLocalQuotesClient;
        private readonly IRequestClient<GetGuildSettingRequest> _getGuildSettingsClient;

        public GuildSettingsService(
            IRequestClient<SetSaveReactionRequest> setSaveReactionClient,
            IRequestClient<SetLocalQuotesRequest> setUsingLocalQuotesClient,
            IRequestClient<GetGuildSettingRequest> getGuildSettingsClient)
        {
            _setSaveReactionClient = setSaveReactionClient;
            _setUsingLocalQuotesClient = setUsingLocalQuotesClient;
            _getGuildSettingsClient = getGuildSettingsClient;
        }

        public async Task<HandlerResponseCode> SetSaveReaction(ulong guildId, IEmote emote)
        {
            var response = await _setSaveReactionClient.GetResponse<HandlerResponse>(new SetSaveReactionRequest
            {
                GuildId = guildId,
                Emote = emote.ToString(),
            });
            return response.Message.Code;
        }

        public async Task<HandlerResponseCode> SetUsingLocalQuotes(ulong guildId, bool local)
        {
            var response = await _setUsingLocalQuotesClient.GetResponse<HandlerResponse>(new SetLocalQuotesRequest
            {
                GuildId = guildId,
                UsingLocal = local,
            });
            return response.Message.Code;
        }

        public async Task<bool> IsSaveReaction(ulong guildId, IEmote emote)
        {
            var settings = await GetGuildSettings(guildId);
            return settings.SaveReaction == emote.ToString();
        }

        public async Task<GuildSettings> GetGuildSettings(ulong guildId)
        {
            var response = await _getGuildSettingsClient.GetResponse<GuildSettings>(new GetGuildSettingRequest
            {
                GuildId = guildId,
            });
            return response.Message;
        }
    }
}
