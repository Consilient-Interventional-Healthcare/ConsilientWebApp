using NPOI.SS.UserModel;

namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    internal interface ICellValueExtractor<TCell>
    {
        string GetString(TCell? cell);
        int GetInt(TCell? cell);
        int? GetNullableInt(TCell? cell);
        DateTime GetDateTime(TCell? cell);
        DateTime? GetNullableDateTime(TCell? cell);
        bool GetBoolean(TCell? cell);
        double GetDouble(TCell? cell);
    }
}