namespace Consilient.EmailMonitor.Contracts
{
    public interface IMonitor
    {
        Task MonitorEmailAsync(CancellationToken cancellationToken);
    }
}