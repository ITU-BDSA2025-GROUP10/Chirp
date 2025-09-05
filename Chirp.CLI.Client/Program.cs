using SimpleDB;
using docopt;
namespace Chirp.CLI.Client;
class Program
{
    static void Main()
    {
        CSVDatabase<Cheep> db = new CSVDatabase<Cheep>();
        var cheeps = db.Read();

        UserInterface.printCheep(cheeps);
    }
}