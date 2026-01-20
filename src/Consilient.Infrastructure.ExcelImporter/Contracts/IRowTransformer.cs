namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    public interface IRowTransformer<TRow> where TRow : class
    {
        TRow Transform(TRow row);
    }
}
