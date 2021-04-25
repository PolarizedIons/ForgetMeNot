using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ForgetMeNot.Commands;
using ForgetMeNot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ForgetMeNot
{
    public class App : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly DiscordHandler _discordHandler;

        public App(IServiceScopeFactory scopeFactory, IConfiguration config, DiscordSocketClient discord, DiscordHandler discordHandler)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _discord = discord;
            _discordHandler = discordHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                Log.Information("Migrating Database...");
                await db.Database.MigrateAsync(cancellationToken: cancellationToken);
            }

            Log.Debug("Initializing command handler");
            await _discordHandler.InitializeAsync();

            Log.Information("Logging in...");
            await _discord.LoginAsync(TokenType.Bot, _config["Bot:Token"]);
            await _discord.StartAsync();

            if (!string.IsNullOrWhiteSpace(_config["Bot:Status"]))
            {
                await _discord.SetActivityAsync(new Game(_config["Bot:Status"]));
            }

            Log.Information("Bot started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.StopAsync();
            await _discord.LogoutAsync();
        }
    }
}
