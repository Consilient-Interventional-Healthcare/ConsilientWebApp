using Consilient.EmailMonitor.Contracts;
using MailKit;

namespace Consilient.EmailMonitor.Processors
{
    internal abstract class EmailProcessorBase : IEmailProcessor
    {
        public Task Process(IMessageSummary message)
        {
            if (!MustProcessEmail(message))
            {
                return Task.CompletedTask;
            }
            return ProcessEmail(message);
        }

        protected abstract Task ProcessEmail(IMessageSummary message);
        protected abstract bool MustProcessEmail(IMessageSummary message);
    }
}