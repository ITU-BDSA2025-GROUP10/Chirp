using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using CsvHelper;


class Program
{
    public record Cheep(string Author, string Message, long Timestamp);
    static void Main()
    {
        DateTimeOffset localTime = DateTimeOffset.Now;

        using (var reader = new StreamReader(@"Data/chirp_cli_db.csv"))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var cheeps = csv.GetRecords<Cheep>();
            foreach (var cheep in cheeps)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();
            var timeString = time.ToString("dd/MM/yy HH:mm:ss");
            Console.WriteLine($"{cheep.Author} @ {timeString}: {cheep.Message}");
        }
        }
        
    }
}