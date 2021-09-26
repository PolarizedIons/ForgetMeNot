using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using ForgetMeNot.Common.Transport;
using MassTransit;

namespace ForgetMeNot.DiscordBot.Handlers
{
    public class LoginDiscordHandler : IConsumer<LoginUserRequest>
    {
        public async Task Consume(ConsumeContext<LoginUserRequest> context)
        {
            var req = context.Message;
            var myClient = new DiscordRestClient();
            await myClient.LoginAsync(TokenType.Bearer, req.AuthToken);
            var user = myClient.CurrentUser;
            await context.RespondAsync(new DiscordUser
            {
                Discriminator = user.Discriminator,
                Id = user.Id.ToString(),
                ProfileUrl = user.GetAvatarUrl(),
                Username = user.Username,
            });
        }
    }
}
