using System;
using System.Threading;
using System.Threading.Tasks;
using ForgetMeNot.Core.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ForgetMeNot.Core
{
    class App : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBusControl _busControl;

        public App(IServiceScopeFactory scopeFactory, IBusControl busControl)
        {
            _scopeFactory = scopeFactory;
            _busControl = busControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            Log.Information("Migrating Database...");
            await db.Database.MigrateAsync(cancellationToken: cancellationToken);

            await _busControl.StartAsync(cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _busControl.StopAsync(cancellationToken: cancellationToken);
        }
    }
}
