using Consilient.Infrastructure.ExcelImporter.Core;

namespace Consilient.ProviderAssignments.Contracts
{
    public interface ISinkProvider
    {
        IDataSink GetSink();
    }
}