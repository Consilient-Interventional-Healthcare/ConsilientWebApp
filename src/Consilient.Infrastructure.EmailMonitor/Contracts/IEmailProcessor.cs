using MailKit;

namespace Consilient.Infrastructure.EmailMonitor.Contracts
{
    public interface IEmailProcessor
    {
        Task Process(IMessageSummary message);
    }
}
