using Microsoft.VisualBasic.FileIO;
using (TextFieldParser parser = new TextFieldParser(@"Data/chirp_cli_db.csv"))
{
    parser.TextFieldType = FieldType.Delimited;
    parser.SetDelimiters(",");
    List<Cheep> cheeps = new List<Cheep>();
    List<string> lines = new List<string>();
    while (!parser.EndOfData)
    {
        string[] fields = parser.ReadFields();
        foreach (string field in fields)
        {
            lines.Add(field);
        }
    }
    for (int i = 3; i < lines.Count - 3 + 1; i += 3) {
        Cheep tempCheep = new Cheep(lines[i], lines[i + 1], lines[i + 2]);
        cheeps.Add(tempCheep);
    }

    foreach (Cheep cheep in cheeps)
    {
        Console.WriteLine(cheep);

    }
    
}
public record Cheep(string Author, string message, string timeStamp);