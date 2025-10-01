using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


app.MapGet("/cheeps", () =>
{
    var repo = CSVDatabase<SimpleDB.Cheep>.Instance;   // reuse your CSV DB
    var cheeps = repo.Read();                  // IEnumerable<Cheep>
    return Results.Ok(cheeps);                 // serialize as JSON
});

app.MapPost("/cheep", (Cheep cheep) =>
{
    CSVDatabase<Cheep>.Instance.Store(cheep!);
    return Results.Ok(cheep);
});

app.Run();