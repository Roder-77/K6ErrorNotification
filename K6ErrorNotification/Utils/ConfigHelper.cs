using Microsoft.Extensions.Configuration;

namespace K6ErrorNotification.Utils
{
    public class ConfigHelper
    {
        public static IConfiguration Config = new ConfigurationBuilder()
#if DEBUG
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#else
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#endif
            .Build();
    }
}
