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
    
    
    // Create a new author with an email and name, and sets the id automatically 
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
    
    // Delete an author from the database.
    public async Task DeleteAuthorAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.Cheeps)
            .Include(a => a.Following)
            .Include(a => a.Followers)
            .FirstOrDefaultAsync(a => a.AuthorId == id);

        if (author == null)
        {
            throw new KeyNotFoundException($"Author with id {id} does not exist");
        }

        // Manually delete Following relationships (NoAction behavior)
        _db.Followings.RemoveRange(author.Following);
        _db.Followings.RemoveRange(author.Followers);

        // Delete all comments by this author
        var comments = await _db.Comments
            .Where(c => c.AuthorId == id)
            .ToListAsync();
        _db.Comments.RemoveRange(comments);

        // Cheeps will cascade delete (along with their comments)
        // Now delete the author
        _db.Authors.Remove(author);

        await _db.SaveChangesAsync();
    }
    
    public async Task DeleteAuthorByEmailAsync(string email)
    {
        var authorId = await getAuthorByEmailAsync(email);
        await DeleteAuthorAsync(authorId);
    }
    
    // Create list of whom the user is following
    // Create relation: followerId follows followedId
    public async Task CreateFollowingAsync(int followerId, int followedId)
    {
        // Optional: validate both authors exist
        var followerExists = await _db.Authors.AnyAsync(a => a.AuthorId == followerId);
        var followedExists = await _db.Authors.AnyAsync(a => a.AuthorId == followedId);

        if (!followerExists || !followedExists)
        {
            throw new KeyNotFoundException("Follower or followed author does not exist");
        }

        // Avoid duplicates
        var alreadyFollowing = await _db.Followings.AnyAsync(f =>
            f.FollowerId == followerId && f.FollowedId == followedId);

        if (alreadyFollowing)
        {
            return; // can be thrown if not necessary
        }

        var following = new Following
        {
            FollowerId = followerId,
            FollowedId = followedId
        };

        _db.Followings.Add(following);
        await _db.SaveChangesAsync();
    }
    
    public async Task DeleteFollowingAsync(int followerId, int followedId)
    {
        var follow = await _db.Followings
            .SingleOrDefaultAsync(f =>
                f.FollowerId == followerId &&
                f.FollowedId == followedId);

        if (follow != null)
        {
            _db.Followings.Remove(follow);
            await _db.SaveChangesAsync();
        }
    }
    
    public async Task<Author> GetAuthorWithFollowingAsync(int authorId)
    {
        return await _db.Authors
            .Include(a => a.Following)
            .ThenInclude(f => f.Followed)
            .FirstOrDefaultAsync(a => a.AuthorId == authorId);
    }
}
