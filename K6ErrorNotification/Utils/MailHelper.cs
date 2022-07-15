using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace K6ErrorNotification.Utils
{
    public class MailHelper
    {
        private readonly ILogger<MailHelper> _logger = LogHelper.SetLogger<MailHelper>();
        private readonly string _source = string.Empty;
        private readonly string _sourceArn = string.Empty;
        private readonly IConfiguration _config;
        
        public MailHelper(string fromLocalPart)
        {
            _config = ConfigHelper.Config;
            _source = $"{fromLocalPart}@{_config["Mail:Source:Domain"]}";
            _sourceArn = _config["Mail:Source:Arn"];
        }

        /// <summary>
        /// 發信
        /// </summary>
        /// <param name="subject">主旨</param>
        /// <param name="body">內容</param>
        /// <param name="carbonCopies">副本列表</param>
        /// <param name="blindCarbonCopies">密件副本列表</param>
        public void SendMail(string subject, string body, List<string> carbonCopies = null!, List<string> blindCarbonCopies = null!)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subject))
                    throw new ArgumentNullException(nameof(subject));

                if (string.IsNullOrWhiteSpace(body))
                    throw new ArgumentNullException(nameof(body));

                var recipients = _config.GetSection("Mail:Recipients").Get<List<string>>();

                if (recipients is null || recipients.Count == 0)
                    throw new ArgumentException($"{nameof(recipients)} is empty");

                using var client = new AmazonSimpleEmailServiceClient(_config["Mail:AccessKeyId"], _config["Mail:SecretAccessKey"], RegionEndpoint.APNortheast1);
                var sendRequest = new SendEmailRequest
                {
                    Source = _source,
                    SourceArn = _sourceArn,
                    Destination = new Destination
                    {
                        ToAddresses = recipients,
                        CcAddresses = carbonCopies,
                        BccAddresses = blindCarbonCopies
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            }
                        }
                    }
                };

                var result = client.SendEmailAsync(sendRequest).Result;

                if (result.HttpStatusCode != HttpStatusCode.OK)
                    _logger.LogWarning($"{nameof(SendMail)} Fail");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SendMail)} Error");
            }
        }
    }
}
