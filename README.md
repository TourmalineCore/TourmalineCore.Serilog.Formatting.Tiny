# TourmalineCore.Serilog.Formatting.Tiny

The library sets custom json logging formatting in the Serilog package.

## Basic usage

### Startup.cs
```csharp
using TourmalineCore.Serilog.Formatting.Tiny;
...

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddTourmalineCoreLogging()
                .AddLoggingValuesGenerator<LoggingValuesGenerator>();

            services.AddControllers();
        }
        ...
    }
```

### Serilog configuration

```csharp
using TourmalineCore.Serilog.Formatting.Tiny;
...

        public static LoggerConfiguration BasicConfiguration(this LoggerConfiguration configuration, string environmentName = null, string applicationName = null)
        {
            var loggerConfiguration = configuration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Filter.ByExcluding(logEvent => logEvent.Properties.Any(p => p.Value.ToString().Contains("health/")))
                .Enrich.FromLogContext();

            if (environmentName is null or "Debug")
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
...
```
