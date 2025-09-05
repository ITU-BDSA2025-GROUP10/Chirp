using System;
using System.Collections.Generic;

namespace Chirp.CLI.Client;

public static class UserInterface
{
    public static void printCheep(IEnumerable<Cheep> cheeps)
    {
        foreach (var cheep in cheeps)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();
            var timeString = time.ToString("dd/MM/yy HH:mm:ss");
            Console.WriteLine($"{cheep.Author} @ {timeString:dd/MM/yy HH:mm:ss}: {cheep.Message}");

        }
    }
}