using Consilient.EmailMonitor.Contracts;
using MimeKit;

namespace Consilient.EmailMonitor.Processors
{
    internal class ExtractAttachmentsEmailProcessor : IEmailProcessor
    {
        public void Process(MimeMessage message)
        {
            if (!message.Attachments.Any())
            {
                return;
            }
        }
    }
}
