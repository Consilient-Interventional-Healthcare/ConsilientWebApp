using Consilient.Infrastructure.ExcelImporter.Contracts;
using NPOI.SS.UserModel;
using System.Globalization;

namespace Consilient.Infrastructure.ExcelImporter.NPOI
{
    public sealed class CellValueExtractor(IFormulaEvaluator? formulaEvaluator = null) : ICellValueExtractor<ICell>
    {
        private readonly IFormulaEvaluator? _formulaEvaluator = formulaEvaluator;

        public string GetString(ICell? cell)
        {
            if (cell == null) return string.Empty;

            if (cell.CellType == CellType.Formula)
            {
                return GetFormulaValueAsString(cell);
            }

            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue?.Trim() ?? string.Empty,
                CellType.Numeric => FormatNumericCellAsString(cell),
                CellType.Boolean => cell.BooleanCellValue ? "TRUE" : "FALSE",
                CellType.Error => string.Empty,
                _ => string.Empty
            };
        }

        public int GetInt(ICell? cell) => GetNullableInt(cell) ?? 0;

        public int? GetNullableInt(ICell? cell)
        {
            if (cell == null) return null;

            if (cell.CellType == CellType.Numeric)
            {
                try
                {
                    return Convert.ToInt32(cell.NumericCellValue);
                }
                catch
                {
                    return null;
                }
            }

            var text = GetString(cell);
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
        }

        public DateTime GetDateTime(ICell? cell) => GetNullableDateTime(cell) ?? default;

        public DateTime? GetNullableDateTime(ICell? cell)
        {
            if (cell == null) return null;

            if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
            {
                return cell.DateCellValue;
            }

            var text = GetString(cell);
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null;
        }

        public bool GetBoolean(ICell? cell)
        {
            if (cell == null) return false;

            if (cell.CellType == CellType.Boolean) return cell.BooleanCellValue;

            var text = GetString(cell);
            return bool.TryParse(text, out var v) && v;
        }

        public double GetDouble(ICell? cell)
        {
            if (cell == null) return 0.0;

            if (cell.CellType == CellType.Numeric) return cell.NumericCellValue;

            var text = GetString(cell);
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v) ? v : 0.0;
        }

        private string GetFormulaValueAsString(ICell cell)
        {
            try
            {
                if (_formulaEvaluator != null)
                {
                    var evaluated = _formulaEvaluator.Evaluate(cell);
                    return evaluated?.CellType switch
                    {
                        CellType.String => evaluated.StringValue?.Trim() ?? string.Empty,
                        CellType.Numeric => evaluated.NumberValue.ToString(CultureInfo.InvariantCulture),
                        CellType.Boolean => evaluated.BooleanValue ? "TRUE" : "FALSE",
                        _ => string.Empty
                    };
                }

                // fallback to cached result type
                return cell.CachedFormulaResultType switch
                {
                    CellType.String => cell.StringCellValue?.Trim() ?? string.Empty,
                    CellType.Numeric => FormatNumericCellAsString(cell),
                    CellType.Boolean => cell.BooleanCellValue ? "TRUE" : "FALSE",
                    _ => string.Empty
                };
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string FormatNumericCellAsString(ICell cell)
        {
            if (DateUtil.IsCellDateFormatted(cell))
            {
                return cell.DateCellValue?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
        }
    }
}