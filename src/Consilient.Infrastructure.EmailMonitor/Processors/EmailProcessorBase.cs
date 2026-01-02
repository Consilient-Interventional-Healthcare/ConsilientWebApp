using Consilient.Infrastructure.EmailMonitor.Contracts;
using MailKit;

namespace Consilient.Infrastructure.EmailMonitor.Processors
{
    internal abstract class EmailProcessorBase : IEmailProcessor
    {
        public Task Process(IMessageSummary message) =>
            MustProcessEmail(message) ? ProcessEmail(message) : Task.CompletedTask;

        protected abstract Task ProcessEmail(IMessageSummary message);
        protected abstract bool MustProcessEmail(IMessageSummary message);
    }
}