using K6ErrorNotification.Utils;
using Microsoft.Extensions.Logging;
using System.Reflection;

#nullable disable

namespace K6ErrorNotification
{
    public class Program
    {
        private static readonly ILogger<Program> _logger = LogHelper.SetLogger<Program>();

        static async Task Main(string[] args)
        {
            // 測試用
            args = new string[] { "NotifyErrorService" };

            if (!args.Any())
            {
                _logger.LogError("未傳入任何參數");
                return;
            }

            var type = Assembly.GetExecutingAssembly().GetExportedTypes()
                .FirstOrDefault(x => x.IsClass && !x.IsAbstract && x.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));

            if (type is null)
            {
                _logger.LogError("查無此功能");
                return;
            }

            var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("Run");

            await (Task)method.Invoke(instance, args.Skip(1).ToArray());
        }
    }
}