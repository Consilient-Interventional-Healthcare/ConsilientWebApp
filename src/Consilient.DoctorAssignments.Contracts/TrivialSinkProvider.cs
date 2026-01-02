using Consilient.Infrastructure.ExcelImporter.Core;

namespace Consilient.DoctorAssignments.Contracts
{
    public sealed class TrivialSinkProvider(IDataSink sink) : ISinkProvider
    {
        private readonly IDataSink _sink = sink;

        public IDataSink GetSink()
        {
            return _sink;
        }
    }
}