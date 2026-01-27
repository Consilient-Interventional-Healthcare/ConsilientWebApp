using System.Diagnostics.CodeAnalysis;

namespace Consilient.Background.Workers.Contracts;

public interface IRecurringWorker : IBackgroundWorker
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    Task Run(CancellationToken cancellationToken);
}
