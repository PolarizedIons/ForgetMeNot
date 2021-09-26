using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ForgetMeNot.Common;
using ForgetMeNot.Common.Extentions;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace ForgetMeNot.DiscordBot
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Logging.SetupLogging();

            Log.Information("Staring ForgetMeNot Discord Bot");
            try
            {
                using var host = CreateHostBuilder(args).Build();
                await host.StartAsync();
                await host.WaitForShutdownAsync();
                await host.StopAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal exception");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostCtx, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", true)
                        .AddJsonFile("appsettings.Development.json", true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumers(typeof(Program).Assembly);
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.ConfigureEndpoints(context);
                            cfg.Host($"rabbitmq://{hostCtx.Configuration["RabbitMq:Host"]}/", configurator =>
                            {
                                configurator.Username(hostCtx.Configuration["RabbitMq:User"]);
                                configurator.Password(hostCtx.Configuration["RabbitMq:Pass"]);
                            });
                        });
                    });
                    services.AddGenericRequestClient();

                    services.AddSingleton(new DiscordSocketClient(
                        new DiscordSocketConfig
                        {
                            MessageCacheSize = 100,
                            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildEmojis | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions,
                            AlwaysDownloadUsers = true
                        }
                    ));

                    services.AddSingleton(new CommandService(new CommandServiceConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        CaseSensitiveCommands = false
                    }));
                    
                    services.DiscoverDiServices();
                    services.AddHostedService<App>();
                })
                .UseSerilog()
                .UseConsoleLifetime();
        }
    }
}
