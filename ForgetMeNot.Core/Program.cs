using System;
using System.IO;
using System.Threading.Tasks;
using ForgetMeNot.Common;
using ForgetMeNot.Common.Extentions;
using ForgetMeNot.Core.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ForgetMeNot.Core
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Logging.SetupLogging();

            Log.Information("Staring ForgetMeNot Core");
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

                    services.AddDbContext<DatabaseContext>(opts =>
                    {
                        var connectionString = hostCtx.Configuration.GetConnectionString("ForgetMeNot");
                        opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                        opts.ConfigureWarnings(c => c.Log(
                            (RelationalEventId.CommandExecuted, LogLevel.Debug),
                            (CoreEventId.ContextInitialized, LogLevel.Debug)
                        ));
                    });

                    services.DiscoverAndMakeDiServicesAvailable();
                    services.AddHostedService<App>();
                })
                .UseSerilog()
                .UseConsoleLifetime();
        }
    }
}