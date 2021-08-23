using System;
using System.Diagnostics;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace ForgetMeNot.Common
{
    public static class Logging
    {
        public static void SetupLogging()
        {
            var loggerBuilder = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext();

            if (Debugger.IsAttached)
            {
                loggerBuilder.WriteTo.Console();
            }
            else
            {
                loggerBuilder.WriteTo.Console(new JsonFormatter());
            }

            var seqUrl = Environment.GetEnvironmentVariable("SEQ__Url");
            var seqApiKey = Environment.GetEnvironmentVariable("SEQ__Key");
            if (!string.IsNullOrEmpty(seqUrl))
            {
                loggerBuilder.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
            }

            Log.Logger = loggerBuilder.CreateLogger();
        }
    }
}
