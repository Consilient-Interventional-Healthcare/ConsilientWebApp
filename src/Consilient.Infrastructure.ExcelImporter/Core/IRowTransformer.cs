namespace Consilient.Infrastructure.ExcelImporter.Core
{
    public interface IRowTransformer<TRow> where TRow : class
    {
        TRow Transform(TRow row);
    }
}
