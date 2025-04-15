using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Common.Helpers
{
    public class CsvService : ICsvService
    {
        private readonly CsvConfiguration _config = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };

        public byte[] WriteCsvToByteArray<T>(IEnumerable<T> records)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, _config);

            csv.WriteHeader<T>();
            csv.NextRecord();
            csv.WriteRecords(records);
            writer.Flush();

            return memoryStream.ToArray();
        }
    }
}
