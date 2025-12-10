using Consilient.Infrastructure.ExcelImporter.Contracts;

namespace Consilient.Infrastructure.ExcelImporter.Helpers
{
    internal class DefaultExcelDataMapper<TData> : IExcelDataMapper<TData> where TData : class, new()
    {
        public IEnumerable<TData> Map(SheetReadResult sheetReadResult)
        {
            // Mapping logic here
            throw new NotImplementedException();
        }
    }
}
