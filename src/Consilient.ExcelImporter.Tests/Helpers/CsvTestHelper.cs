using CsvHelper;
using System.Globalization;

namespace Consilient.ExcelImporter.Tests.Helpers
{
    public static class CsvTestHelper
    {
        public static void WriteToCsv<T>(IEnumerable<T> records, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(records);
        }
    }
}