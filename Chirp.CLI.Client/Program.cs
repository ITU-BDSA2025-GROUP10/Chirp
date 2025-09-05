using SimpleDB;
namespace Chirp.CLI.Client;
class Program
{
    public record Cheep(string Author, string Message, long Timestamp);
    static void Main()
    {
        CSVDatabase<Cheep> db = new CSVDatabase<Cheep>();
        DateTimeOffset localTime = DateTimeOffset.Now;
        var cheeps = db.Read();

        foreach (var cheep in cheeps)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();
            var timeString = time.ToString("dd/MM/yy HH:mm:ss");
            var msg = $"{cheep.Author} @ {timeString}: {cheep.Message}";
            db.Store(cheep);
            Console.WriteLine(msg);
        }
    }
}