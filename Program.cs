using System.Text.RegularExpressions;
using (StreamReader reader = new StreamReader(@"Data/chirp_cli_db.csv"))
{
    string line; 
    List<cheep> cheepList = new List<cheep>();
    while ((line = reader.ReadLine()) != null)
    {
        //Define pattern
        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        //Separating columns to array
        string[] lines = CSVParser.Split(line);
        Console.WriteLine(lines.Length);
        /*for  (int i = 3; i < lines.Length - 3; i += 3) 
        {
           cheep post = new cheep(lines[i], lines[i + 1], DateTimeOffset.Parse(lines[i + 2])); 
           cheepList.Add(post);
        }*/
    }

    /*foreach (cheep cheep in cheepList)
    {
        Console.WriteLine(cheep);
    }*/
    
}
public record cheep(string Author, string message, DateTimeOffset Timestamp);