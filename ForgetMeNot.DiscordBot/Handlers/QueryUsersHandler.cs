using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using ForgetMeNot.Common.Transport;
using MassTransit;

namespace ForgetMeNot.DiscordBot.Handlers
{
    public class QueryUsersHandler : IConsumer<QueryUsersRequest>
    {
        private readonly DiscordSocketClient _discord;

        public QueryUsersHandler(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task Consume(ConsumeContext<QueryUsersRequest> context)
        {
            var users = context.Message.UserIds.Select(x => _discord.GetUser(x));
            await context.RespondAsync(new ListResponse<DiscordUser?>(users.Select(x => x != null 
                ? new DiscordUser
                {
                    Discriminator = x.Discriminator,
                    Id = x.Id.ToString(),
                    Username = x.Username,
                    ProfileUrl = x.GetAvatarUrl(),
                }
                : null
            )));
        }
    }
}
