using SimpleDB;
using DocoptNet;
namespace Chirp.CLI.Client;
class Program
{
    static void Main()
    {
        var db = CSVDatabase<Cheep>.Instance;
        var cheeps = db.Read();

        UserInterface.printCheep(cheeps);
    }
}