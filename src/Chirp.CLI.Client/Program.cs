using System.Net.Http.Json;
using Chirp.CLI.Client;
using DocoptNet;

class Program
{

    const string usage = @"
Chirp Client.

Usage:
  chirp.client cheep <message> [--author=<name>]
  chirp.client cheeps
  chirp.client (-h | --help)
  chirp.client --version

Options:
  -h --help         Show this screen.
  --version         Show version.
  --author=&lt;name&gt;  Author name [default: anonymous].
";

    static async Task Main(string[] args)
    {

        var arguments = new Docopt().Apply(usage, args, version: "Chirp Client 1.0", exit: true);


        var client = new HttpClient { BaseAddress = new Uri("http://localhost:5196") };

        // if "cheep" command
        if (arguments["cheep"].IsTrue)
        {
            var message = arguments["<message>"].ToString()!;
            var author = (arguments.ContainsKey("--author") && arguments["--author"] != null)
                ? arguments["--author"].ToString()!
                : "anonymous";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var cheep = new Cheep(author, message, timestamp);
            var response = await client.PostAsJsonAsync("/cheep", cheep);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
        }
        // if "cheeps" command
        else if (arguments["cheeps"].IsTrue)

        {

            var cheeps = await client.GetStringAsync("/cheeps");
            Console.WriteLine(cheeps);
        }
    }
}