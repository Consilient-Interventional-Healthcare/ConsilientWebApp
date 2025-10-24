using ClosedXML.Excel;
using ExcelDataReader;
using System.Data;
using System.Text;

namespace Consilient.Infrastructure.ExcelImporter.Helpers
{
    /// <summary>
    /// A factory for creating <see cref="IXLWorkbook"/> instances from file paths.
    /// Handles in-memory conversion of legacy .xls files to the .xlsx format.
    /// </summary>
    public static class WorkbookFactory
    {
        /// <summary>
        /// Initializes the <see cref="WorkbookFactory"/> by registering the necessary encoding provider.
        /// </summary>
        static WorkbookFactory()
        {
            // Required for ExcelDataReader to work with .NET Core and later.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Creates an <see cref="IXLWorkbook"/> from the specified file.
        /// </summary>
        /// <param name="filename">The path to the Excel file.</param>
        /// <returns>An <see cref="IXLWorkbook"/> instance.</returns>
        /// <remarks>
        /// If the file is an .xls file, it is converted to .xlsx format in-memory.
        /// The returned workbook should be disposed by the caller.
        /// </remarks>
        public static IXLWorkbook Create(bool canConvertFile, string filename)
        {
            if (canConvertFile && Path.GetExtension(filename).Equals(".xls", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = File.Open(filename, FileMode.Open, FileAccess.Read);
                return ConvertXlsToXlsx(stream);
            }

            // For .xlsx files, ClosedXML can handle the file path directly.
            return new XLWorkbook(filename);
        }

        /// <summary>
        /// Converts a stream from an .xls file into an in-memory .xlsx <see cref="XLWorkbook"/>.
        /// </summary>
        /// <param name="xlsStream">The stream of the .xls file.</param>
        /// <returns>A new <see cref="XLWorkbook"/> instance.</returns>
        private static XLWorkbook ConvertXlsToXlsx(Stream xlsStream)
        {
            using var reader = ExcelReaderFactory.CreateReader(xlsStream);
            var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true // Use first row as header
                }
            });

            var workbook = new XLWorkbook();
            foreach (DataTable table in ds.Tables)
            {
                // Add each DataTable as a new worksheet
                if (!string.IsNullOrEmpty(table.TableName))
                {
                    workbook.Worksheets.Add(table, table.TableName);
                }
            }
            return workbook;
        }
    }
}