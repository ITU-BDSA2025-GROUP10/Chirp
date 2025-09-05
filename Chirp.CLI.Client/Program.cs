using System.Collections.Generic;
using SimpleDB;

namespace Chirp.CLI.Client;

class Program
{
    static void Main()
    {
        var db = new CSVDatabase<Cheep>();
        IEnumerable<Cheep> cheeps = db.Read();

        UserInterface.PrintCheeps(cheeps);
    }
}