using Chirp.Core.Models;

namespace Chirp.Infrastructure.Repositories;

public interface IAuthorRepository
{
    // Return the id of the author with that name
    Task<int> getAuthorByNameAsync(string name);
    // Return the id of the author with that email
    Task<int> getAuthorByEmailAsync(string email);
    // Create a new author with a email and name (id should be set automatically 
    Task<int> createAuthorAsync(string name, string email);
    // Delete a author from the database.
    Task deleteAuthorAsync(int id);
    // Get list of whom the user is following
    Task<List<Author>> GetFollowingAsync();
    // Get list of who is following the user
    Task<List<Author>> GetFollowersAsync();
}
