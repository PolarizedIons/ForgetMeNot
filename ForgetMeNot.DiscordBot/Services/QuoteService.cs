using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using ForgetMeNot.Common.Database.Models;
using ForgetMeNot.Common.Extentions;
using ForgetMeNot.Common.Transport;
using MassTransit;

namespace ForgetMeNot.DiscordBot.Services
{
    public class QuoteService : ISingletonDiService
    {
        private readonly IRequestClient<RandomQuoteRequest> _searchQuoteClient;
        private readonly IRequestClient<ForgetQuoteRequest> _forgetQuoteClient;
        private readonly IRequestClient<RememberQuoteRequest> _rememberQuoteClient;

        public QuoteService(
            IRequestClient<RandomQuoteRequest> searchQuoteClient,
            IRequestClient<ForgetQuoteRequest> forgetQuoteClient,
            IRequestClient<RememberQuoteRequest> rememberQuoteClient)
        {
            _searchQuoteClient = searchQuoteClient;
            _forgetQuoteClient = forgetQuoteClient;
            _rememberQuoteClient = rememberQuoteClient;
        }

        public async Task<HandlerResponseCode> RememberQuote(IUserMessage message)
        {
            var response = await _rememberQuoteClient.GetResponse<HandlerResponse>(new RememberQuoteRequest
            {
                Message = message.Content,
                AuthorId = message.Author.Id,
                ChannelId = message.Channel.Id,
                GuildId = ((IGuildChannel)message.Channel).GuildId,
                MessageId = message.Id
            });
            return response.Message.Code;
        }

        public async Task<HandlerResponseCode> ForgetQuote(IUserMessage message)
        {
            var response = await _forgetQuoteClient.GetResponse<HandlerResponse>(new ForgetQuoteRequest
            {
                MessageId = message.Id,
            });
            return response.Message.Code;
        }

        public async Task<Quote?> GetQuote(IInteractionContext context, IGuildUser? user, string? searchTerm)
        {
            // Don't know why context.Guild is null...
            var channel = context.Channel;
            if (channel is not IGuildChannel guildChannel)
            {
                return null;
            }

            var guild = guildChannel.Guild;

            var response = await _searchQuoteClient.GetResponse<Quote, NotFoundResponse>(new RandomQuoteRequest
                {
                    GuildId = guild.Id,
                    ChannelId = channel.Id,
                    UserId = user?.Id,
                    SearchTerm = searchTerm,
                });
            return response.Message as Quote;
        }
    }
}
