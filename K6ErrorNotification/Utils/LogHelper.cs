using Microsoft.Extensions.Logging;

namespace K6ErrorNotification.Utils
{
    public class LogHelper
    {
        private static readonly ILoggerFactory _loggerFactory;

        static LogHelper() {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("K6ErrorNotification", LogLevel.Debug)
                    .AddLog4Net();
            });
        }

        public static ILogger<T> SetLogger<T>() where T : class => _loggerFactory.CreateLogger<T>();
    }
}
