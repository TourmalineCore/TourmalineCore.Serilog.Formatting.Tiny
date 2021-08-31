using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using TourmalineCore.Serilog.Formatting.Tiny.Formatters;

namespace TourmalineCore.Serilog.Formatting.Tiny.Formatters.Extensions
{
    public static class TourmalineCoreSerilogConfigurationExtensions
    {
        public static Logger CreateBasicLogger()
        {
            return new LoggerConfiguration()
                .BasicConfiguration()
                .WriteTo.File("logs/basic_log.txt")
                .CreateLogger();
        }

        public static LoggerConfiguration FromConfig(this LoggerConfiguration configuration, IConfiguration loggerSettings, string environmentName = null, string applicationName = null)
        {
            return configuration.ReadFrom.Configuration(loggerSettings)
                .BasicConfiguration(environmentName, applicationName);
        }

        public static LoggerConfiguration BasicConfiguration(this LoggerConfiguration configuration, string environmentName = null, string applicationName = null)
        {
            var loggerConfiguration = configuration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Filter.ByExcluding(logEvent => logEvent.Properties.Any(p => p.Value.ToString().Contains("health/")))
                .Enrich.FromLogContext();

            if (environmentName is null | environmentName is "Debug")
            {
                loggerConfiguration.WriteTo.Console();
            }
            else
            {
                loggerConfiguration.WriteTo.Console(new TourmalineCoreRenderedCompactJsonFormatter());

                loggerConfiguration.Enrich.WithProperty("@v", Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

                loggerConfiguration.Enrich.WithProperty("@e", environmentName);

                if (applicationName != null)
                {
                    loggerConfiguration.Enrich.WithProperty("@n", applicationName);
                }
            }

            return loggerConfiguration;
        }
    }
}