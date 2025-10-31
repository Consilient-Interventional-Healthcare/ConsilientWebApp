using System.Diagnostics.CodeAnalysis;

namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    public interface IExcelImporter<out TData> where TData : class
    {
        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        IEnumerable<TData> Import(string filename);
    }
}
