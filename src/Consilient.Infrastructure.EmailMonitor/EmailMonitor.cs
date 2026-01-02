using Consilient.Infrastructure.EmailMonitor.Contracts;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Logging;

namespace Consilient.Infrastructure.EmailMonitor
{
    public class EmailMonitor(
        MonitorConfiguration configuration,
        IEnumerable<IEmailProcessor> emailProcessors,
        ILogger<EmailMonitor> logger
        ) : IEmailMonitor
    {
        public async Task MonitorEmailAsync(CancellationToken cancellationToken = default)
        {
            if (!emailProcessors.Any())
            {
                throw new InvalidOperationException("No email processors are configured.");
            }
            using var client = new ImapClient();
            await client.ConnectAsync(configuration.Host, configuration.Port, configuration.UseSsl, cancellationToken);
            await client.AuthenticateAsync(configuration.UserName, configuration.Password, cancellationToken);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
            var deliveredAfterDate = DateTime.Today.AddDays(Math.Abs(configuration.SearchLookbackDays) * -1);
            var query = SearchQuery.DeliveredAfter(deliveredAfterDate);
            var messageIds = await inbox.SearchAsync(query, cancellationToken: cancellationToken);
            var messages = await inbox.FetchAsync(messageIds, MessageSummaryItems.Full | MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure, cancellationToken: cancellationToken);
            //var messages = await inbox.FetchAsync(0, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure, cancellationToken: cancellationToken);
            foreach (var message in messages)
            {
                logger.LogInformation("Found a new message for {TargetEmail}: {Subject}", message.UniqueId, message.NormalizedSubject);
                foreach (var ep in emailProcessors)
                {
                    await ep.Process(message);
                }
            }
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
