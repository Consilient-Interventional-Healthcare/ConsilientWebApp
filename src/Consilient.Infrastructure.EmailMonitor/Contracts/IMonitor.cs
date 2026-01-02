namespace Consilient.Infrastructure.EmailMonitor.Contracts
{
    public interface IEmailMonitor
    {
        Task MonitorEmailAsync(CancellationToken cancellationToken);
    }
}