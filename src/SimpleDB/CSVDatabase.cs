using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SimpleDB;
public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private readonly string _filePath = @"../../Data/chirp_cli_db.csv";
    private static CSVDatabase<T>? _instance;
    private CSVDatabase() { }
    public static CSVDatabase<T> Instance => _instance ??= new CSVDatabase<T>();
    public IEnumerable<T> Read(int? limit = null)
    {
        using (var reader = new StreamReader(_filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<T>();
            return records.ToList(); // materialiserer enumerationen, så filen kan lukkes
        }
    }
    public void Store(T record)
    {
        using (var writer = new StreamWriter(_filePath, append: true))
        using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteRecord(record);
            csvWriter.NextRecord(); // Sørg for at skrive en ny linje efter hver post
        }

    }
}