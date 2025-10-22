namespace Consilient.EmailMonitor.Contracts
{
    public interface IEmailMonitor
    {
        Task MonitorEmailAsync(CancellationToken cancellationToken);
    }
}