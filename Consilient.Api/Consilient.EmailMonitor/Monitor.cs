using Consilient.EmailMonitor.Contracts;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Logging;

namespace Consilient.EmailMonitor
{
    public class Monitor(MonitorConfiguration configuration, ILoggerFactory loggerFactory) : IMonitor
    {
        private readonly MonitorConfiguration Configuration = configuration;
        private readonly ILogger<Monitor> _logger = loggerFactory.CreateLogger<Monitor>();
        private readonly List<IEmailProcessor> EmailProcessors = [];

        public async Task MonitorEmailAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(Configuration.TargetEmail))
            {

            }
            if (EmailProcessors.Count == 0)
            {

            }
            using var client = new ImapClient();
            await client.ConnectAsync(Configuration.Host, Configuration.Port, Configuration.UseSSL, cancellationToken);
            await client.AuthenticateAsync(Configuration.UserName, Configuration.Password, cancellationToken);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            var previousCount = inbox.Count;

            inbox.CountChanged += async (sender, e) =>
            {
                if (sender is not IMailFolder folder)
                {
                    return;
                }
                if (folder.Count > previousCount)
                {
                    var newMessagesCount = folder.Count - previousCount;
                    _logger.LogInformation("{Count} new messages arrived.", newMessagesCount);

                    for (int i = previousCount; i < folder.Count; i++)
                    {
                        var message = await folder.GetMessageAsync(i, cancellationToken);
                        if (message.From.Any(a => a is MimeKit.InternetAddress address && address.Name == Configuration.TargetEmail))
                        {
                            _logger.LogInformation("Found a new message for {TargetEmail}: {Subject}", Configuration.TargetEmail, message.Subject);
                            foreach (var ep in EmailProcessors)
                            {
                                ep.Process(message);
                            }
                        }
                    }
                }
                previousCount = folder.Count;
            };

            var done = new CancellationTokenSource();
            cancellationToken.Register(done.Cancel);

            await client.IdleAsync(done.Token);

            await client.DisconnectAsync(true, CancellationToken.None);
        }
    }
}
