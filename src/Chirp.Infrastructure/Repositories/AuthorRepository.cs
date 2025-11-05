using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly ChatDBContext _db;
    
    public AuthorRepository(ChatDBContext db)
    {
        _db = db;
    }
    
    // Return the id of the author with that name
    //TODO (What if more than one author has the same name? Maybe return a list instead?)
    public async Task<int> getAuthorByNameAsync(string name)
    {
        var author = await _db.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Name == name);
        if (author is null)
        {
            throw new KeyNotFoundException($"Author with name '{name}' does not exist");
        }

        return author.AuthorId;
    }
    
    // Return the id of the author with that email
    public async Task<int> getAuthorByEmailAsync(string email)
    {
        var author = await _db.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == email);
        if (author is null)
        {
            throw new KeyNotFoundException($"Author with name '{email}' does not exist");
        }

        return author.AuthorId;
    }
    
    
    // Create a new author with a email and name, and sets the id automatically 
    public async Task<int> createAuthorAsync(string name, string email)
    {
        var author = new Author
        {
            Name = name,
            Email = email,
        };
        _db.Authors.Add(author);
        
        await _db.SaveChangesAsync();
        
        return author.AuthorId;
    }
    
    // Delete a author from the database.
    public async Task deleteAuthorAsync(int id)
    {
        _db.Authors.Remove(await _db.Authors.FindAsync(id));
        await _db.SaveChangesAsync();
    }
}
