using MailKit;

namespace Consilient.EmailMonitor.Contracts
{
    public interface IEmailProcessor
    {
        Task Process(IMessageSummary message);
    }
}
