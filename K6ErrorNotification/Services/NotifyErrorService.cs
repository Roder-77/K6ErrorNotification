using K6ErrorNotification.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace K6ErrorNotification.Services
{
    public class NotifyErrorService
    {
        private readonly ILogger<NotifyErrorService> _logger = LogHelper.SetLogger<NotifyErrorService>();

        public void Run()
        {
            var now = DateTime.Now;
            _logger.LogInformation($"{now}, {nameof(NotifyErrorService)} Run Start");

            try
            {
                var nowDate = now.ToString("yyyyMMdd");
                var k6BasePath = ConfigHelper.Config["K6:BasePath"];
                var fileNames = ConfigHelper.Config.GetSection("K6:FileNames").Get<string[]>();
                var mailHelper = new MailHelper("K6Test");

                foreach (var fileName in fileNames)
                {
                    var filePath = Path.Combine(k6BasePath, $"{fileName}");

                    // 檔案不存在
                    if (!File.Exists(filePath))
                        continue;

                    var content = File.ReadAllLines(filePath);

                    // 內容為空
                    if (content is null || content.Length == 0)
                        continue;

                    var prefix = fileName.Split('.')[0];
                    var directoryPath = Path.Combine(ConfigHelper.Config["K6:OutputPath"], $"{prefix}Logs");

                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    var outputPath = Path.Combine(directoryPath, $"{nowDate}_Log.txt");

                    File.AppendAllLines(outputPath, content.Append(""));
                    // 輸出完後刪除原檔
                    File.Delete(filePath);

                    IEnumerable<string> apiDetails;

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        apiDetails = content.Where(x => x.Contains("ERRO"));
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        apiDetails = content.Where(x => x.Contains("level=error"));
                    else
                        apiDetails = content;

                    apiDetails = apiDetails
                        .Where(x => x.Contains('|'))
                        .Select(x =>
                        {
                            var arr = x.Split('|');
                            return $"{arr[1]}：{arr[2]}";
                        });

                    // 未包含錯誤的 API 詳細資訊
                    if (apiDetails is null || !apiDetails.Any())
                        continue;

                    mailHelper.SendMail($"{prefix} 測試 API 發生異常", $"異常資訊：<br>{string.Join("<br>", apiDetails)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(NotifyErrorService)} Run Error");
                throw;
            }
        }
    }
}
