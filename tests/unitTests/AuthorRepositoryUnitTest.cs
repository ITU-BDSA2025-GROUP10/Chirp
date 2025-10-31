namespace DefaultNamespace;

public class AuthorRepositoryUnitTest
{
// Arrange
using var connection = new SqliteConnection("Filename=:memory:");
await connection.OpenAsync();
var builder = new DbContextOptionsBuilder<ChirpContext>().UseSqlite(connection);

using var context = new ChirpContext(builder.Options);
await context.Database.EnsureCreatedAsync(); // Applies the schema to the database

IMessageRepository repository = new MessageRepository(context);

// Act
var result = repository.QueryMessages("TestUser");

[fact]

}
