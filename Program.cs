List<string> cheeps = new () { "hello, BDSA students!", "Welcome to the course", "i hope you had a good summer"};

foreach (var cheep in cheeps)
{
    Console.WriteLine(cheep);
    Thread.Sleep(1000);
}