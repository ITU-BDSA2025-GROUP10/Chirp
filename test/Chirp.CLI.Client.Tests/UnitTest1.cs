using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Chirp.CLI.Client;
using System.Globalization;

namespace Chirp.CLI.Client.Tests;

public class UnitTest1
{
    [Fact]
    public void testUserInterfaceCheepPrinting()
    {
        var cheep = new Cheep("Master Splinter", "Welcome to the course!", 1690978778);
        var cheeps = new List<Cheep> { cheep };

        var writer = new StringWriter();
        Console.SetOut(writer);

        UserInterface.printCheep(cheeps);
        var output = writer.ToString().Trim();

        var time = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();
        var timeString = time.ToString("dd'/'MM'/'yy HH':'mm':'ss", CultureInfo.InvariantCulture);
        var expected = $"{cheep.Author} @ {timeString}: {cheep.Message}";
        Assert.Equal(expected, output);
    }
}

