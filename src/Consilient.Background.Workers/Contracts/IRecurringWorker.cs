namespace Consilient.Background.Workers.Contracts
{
    public interface IRecurringWorker : IBackgroundWorker
    {
        Task Run(CancellationToken cancellationToken);
    }
}
