using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Consilient.Infrastructure.ExcelImporter.Helpers
{
    /// <summary>
    /// A factory for creating <see cref="IWorkbook"/> instances from file paths.
    /// Uses NPOI to read both .xls and .xlsx files. When requested, converts .xls streams
    /// into an in-memory .xlsx workbook.
    /// </summary>
    public static class WorkbookFactory
    {
        static WorkbookFactory()
        {
            // No special encoding provider required for NPOI; constructor kept for parity.
        }

        /// <summary>
        /// Creates an <see cref="IWorkbook"/> from the specified file.
        /// </summary>
        /// <param name="canConvertFile">Indicates whether .xls files should be converted into an XSSFWorkbook.</param>
        /// <param name="fileName">The path to the Excel file.</param>
        /// <returns>An <see cref="IWorkbook"/> instance. Caller is responsible for disposing it.</returns>
        public static IWorkbook Create(bool canConvertFile, string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Excel file not found.", fileName);
            }

            // Read the file fully into memory first to avoid locked-file/blocking behavior
            var fileBytes = File.ReadAllBytes(fileName);

            if (!CanConvertFile(canConvertFile, fileName))
            {
                // Let NPOI detect and create the appropriate workbook implementation
                using var ms = new MemoryStream(fileBytes);
                return NPOI.SS.UserModel.WorkbookFactory.Create(ms);
            }

            using var xlsStream = new MemoryStream(fileBytes);
            return ConvertXlsToXlsx(xlsStream);
        }

        private static bool CanConvertFile(bool canConvertFile, string fileName)
        {
            return canConvertFile && Path.GetExtension(fileName).Equals(".xls", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts a stream from an .xls file into an in-memory XSSFWorkbook.
        /// This performs a value-by-value copy of sheets/rows/cells (styles are not preserved).
        /// </summary>
        /// <param name="xlsStream">The stream of the .xls file.</param>
        /// <returns>A new <see cref="XSSFWorkbook"/> instance.</returns>
        private static XSSFWorkbook ConvertXlsToXlsx(Stream xlsStream)
        {
            // Read the old-format workbook
            var hssf = new HSSFWorkbook(xlsStream);
            var xssf = new XSSFWorkbook();

            for (int s = 0; s < hssf.NumberOfSheets; s++)
            {
                var hSheet = hssf.GetSheetAt(s);
                var sheetName = string.IsNullOrEmpty(hSheet.SheetName) ? $"Sheet{s + 1}" : hSheet.SheetName;
                var xSheet = xssf.CreateSheet(sheetName);

                // Copy rows and cells (values only)
                for (int r = hSheet.FirstRowNum; r <= hSheet.LastRowNum; r++)
                {
                    var hRow = hSheet.GetRow(r);
                    if (hRow == null)
                    {
                        continue;
                    }

                    var xRow = xSheet.CreateRow(hRow.RowNum);
                    for (int c = hRow.FirstCellNum; c < hRow.LastCellNum; c++)
                    {
                        if (c < 0) continue; // safeguard for some HSSF implementations
                        var hCell = hRow.GetCell(c);
                        if (hCell == null) continue;

                        var xCell = xRow.CreateCell(hCell.ColumnIndex);
                        switch (hCell.CellType)
                        {
                            case CellType.String:
                                xCell.SetCellValue(hCell.StringCellValue);
                                break;
                            case CellType.Numeric:
                                xCell.SetCellValue(hCell.NumericCellValue);
                                break;
                            case CellType.Boolean:
                                xCell.SetCellValue(hCell.BooleanCellValue);
                                break;
                            case CellType.Formula:
                                // Preserve formula text; evaluation may be done by caller if needed
                                xCell.CellFormula = hCell.CellFormula;
                                break;
                            case CellType.Blank:
                                xCell.SetCellValue(string.Empty);
                                break;
                            case CellType.Error:
                                xCell.SetCellErrorValue(hCell.ErrorCellValue);
                                break;
                            default:
                                // Fallback to string representation
                                xCell.SetCellValue(hCell.ToString());
                                break;
                        }
                    }
                }
            }

            return xssf;
        }
    }
}