using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Helpers;
using Microsoft.Extensions.Logging;

namespace Consilient.Infrastructure.ExcelImporter
{
    public abstract class ExcelImporterBase<TData>(IExcelSheetReader excelSheetReader, ExcelImporterConfiguration configuration, ILogger logger) : IExcelImporter<TData>
        where TData : class, new()
    {
        protected ILogger Logger { get; } = logger;
        protected ExcelImporterConfiguration Configuration { get; init; } = configuration;
        private readonly IExcelSheetReader _excelSheetReader = excelSheetReader ?? throw new InvalidOperationException();
        private readonly DefaultExcelDataMapper<TData> _mapper = new();

        public IEnumerable<TData> Import(string filename)
        {
            Logger.LogInformation("Starting Excel import for file: {FileName}", filename);
            var results = _excelSheetReader.ReadSheet(filename, Configuration);
            var mappedResults = _mapper.Map(results);
            Logger.LogInformation("Excel import finished for file: {FileName}", filename);
            return mappedResults;
        }
    }
}