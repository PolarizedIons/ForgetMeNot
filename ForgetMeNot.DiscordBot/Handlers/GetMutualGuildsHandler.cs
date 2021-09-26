using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ForgetMeNot.Common.Transport;
using MassTransit;

namespace ForgetMeNot.DiscordBot.Handlers
{
    public class GetMutualGuildsHandler : IConsumer<GetMutualGuildsRequest>
    {
        private readonly DiscordSocketClient _discord;

        public GetMutualGuildsHandler(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task Consume(ConsumeContext<GetMutualGuildsRequest> context)
        {
            var user = _discord.GetUser(context.Message.UserId);
            var guilds = user == null
                ? Array.Empty<DiscordGuild>()
                : user.MutualGuilds.Select(guild => new DiscordGuild
                    {
                        Channels = guild.Channels
                            .Where(channel => channel is ITextChannel && channel.Users.Any(x => x.Id == context.Message.UserId))
                            .OrderBy(channel => channel.Position)
                            .Select(channel => new DiscordChannel
                            {
                                Id = channel.Id.ToString(),
                                Name = channel.Name,
                            }),
                        Id = guild.Id.ToString(),
                        Name = guild.Name,
                        IconUrl = guild.IconUrl,
                    }
                );

            await context.RespondAsync(new ListResponse<DiscordGuild>()
            {
                List = guilds,
            });
        }
    }
}
