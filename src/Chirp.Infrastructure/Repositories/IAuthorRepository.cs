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
    // Delete an author from the database.
    Task DeleteAuthorAsync(int id);
    // Delete an author by email.
    Task DeleteAuthorByEmailAsync(string email);
    
    Task CreateFollowingAsync(int followerId, int followedId);
    
    Task<Author> GetAuthorWithFollowingAsync(int authorId);
}
