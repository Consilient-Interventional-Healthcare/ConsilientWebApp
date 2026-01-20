using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;

namespace Consilient.ProviderAssignments.Services.Import.Sinks
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