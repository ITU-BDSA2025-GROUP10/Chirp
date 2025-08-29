if (args[0] == "say")
{
    var message = args[1];
    var frequency = int.Parse(args[2]);
    foreach (var i in Enumerable.Range(1, frequency))
        Console.Write(message + " ");
}