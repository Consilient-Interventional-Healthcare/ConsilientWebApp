using Consilient.Infrastructure.ExcelImporter.Core;

namespace Consilient.DoctorAssignments.Contracts
{
    public interface ISinkProvider
    {
        IDataSink GetSink();
    }
}