using MimeKit;

namespace Consilient.EmailMonitor.Contracts
{
    public interface IEmailProcessor
    {
        void Process(MimeMessage message);
    }
}
