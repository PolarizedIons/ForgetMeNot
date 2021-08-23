using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ForgetMeNot.DiscordBot.Commands;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ForgetMeNot.DiscordBot
{
    public class App : IHostedService
    {
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly DiscordHandler _discordHandler;
        private readonly IBusControl _busControl;

        public App(IConfiguration config, DiscordSocketClient discord, DiscordHandler discordHandler, IBusControl busControl)
        {
            _config = config;
            _discord = discord;
            _discordHandler = discordHandler;
            _busControl = busControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Debug("Initializing command handler");
            await _discordHandler.InitializeAsync();

            Log.Information("Logging in...");
            await _discord.LoginAsync(TokenType.Bot, _config["Bot:Token"]);
            await _discord.StartAsync();

            if (!string.IsNullOrWhiteSpace(_config["Bot:Status"]))
            {
                await _discord.SetActivityAsync(new Game(_config["Bot:Status"]));
            }
            
            await _busControl.StartAsync(cancellationToken: cancellationToken);
            
            Log.Information("Bot started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.StopAsync();
            await _discord.LogoutAsync();
            await _busControl.StopAsync(cancellationToken: cancellationToken);
        }
    }
}
