using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Chirp.CLI.Client;

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

        Assert.Equal("Master Splinter @ 02/08/23 14:19:38: Welcome to the course!", output);
    }
}

