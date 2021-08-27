using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TourmalineCore.Serilog.Formatting.Tiny.Extentions
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddTourmalineCoreLogging(this IServiceCollection services)
        {
            return services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
        }
    }
}