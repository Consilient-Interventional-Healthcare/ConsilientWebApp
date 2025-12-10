namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    public interface IExcelImporter<out TData> where TData : class
    {
        IEnumerable<TData> Import(string filename);
    }
}
