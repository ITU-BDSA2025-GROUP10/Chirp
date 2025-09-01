using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        DateTimeOffset localTime = DateTimeOffset.Now;

        try
        {
            using (StreamReader reader = new StreamReader(@"Data/chirp_cli_db.csv"))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    string[] X = CSVParser.Split(line);

                    long unixTime;
                    if (long.TryParse(X[2], out unixTime))
                    {
                        DateTimeOffset utcTime = DateTimeOffset.FromUnixTimeSeconds(unixTime);
                        string formatted = utcTime.ToLocalTime().ToString("MM-dd HH:mm:ss");

                        Console.WriteLine($"{X[0]} @ {formatted}: {X[1]}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid timestamp");
                    }
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }
}