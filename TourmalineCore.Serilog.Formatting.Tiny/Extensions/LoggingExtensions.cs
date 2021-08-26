using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TourmalineCore.Logging.Extensions.NetCore.Extensions
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddTourmalineCoreLogging(this IServiceCollection services)
        {
            return services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
        }
    }
}