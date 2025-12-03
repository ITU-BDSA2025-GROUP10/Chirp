using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Service;



public class CheepService : ICheepService
{
    private readonly ChatDBContext _db;

    public CheepService(ChatDBContext db) => _db = db;

    private List<int> GetFollowedIds(string? currentAuthor)
    {
        if (string.IsNullOrEmpty(currentAuthor))
            return new List<int>();

        var author = _db.Authors
            .AsNoTracking()
            .SingleOrDefault(a => a.Name == currentAuthor);

        if (author is null)
            return new List<int>();

        var currentAuthorId = author.AuthorId;

        return _db.Followings
            .Where(f => f.FollowerId == currentAuthorId)
            .Select(f => f.FollowedId)
            .ToList();
    }
    public List<CheepViewModel> GetCheeps(int page = 0, int pageSize = 32)
        => GetCheeps(null, page, pageSize);

    public List<CheepViewModel> GetCheeps(string? currentAuthor, int page = 0, int pageSize = 32)
    {
        var followedIds = GetFollowedIds(currentAuthor);

        return _db.Cheeps
            .AsNoTracking()
            .Include(m => m.Author)
            .Include(m => m.Comments)
            .OrderByDescending(m => m.TimeStamp)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(m => new CheepViewModel(
                m.CheepId,
                m.Author.Name,
                m.Text,
                m.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                m.Comments.Count,
                followedIds.Contains(m.AuthorId) // <- true only if you follow this author
            ))
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 0, int pageSize = 32)
        => GetCheepsFromAuthor(author, null, page, pageSize);
    public List<CheepViewModel> GetCheepsFromAuthor(
        string author,
        string? currentAuthor,
        int page = 0,
        int pageSize = 32)
    {
        var followedIds = GetFollowedIds(currentAuthor);

        return _db.Cheeps
            .AsNoTracking()
            .Include(m => m.Author)
            .Include(m => m.Comments)
            .Where(m => m.Author.Name == author)
            .OrderByDescending(m => m.TimeStamp)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(m => new CheepViewModel(
                m.CheepId,
                m.Author.Name,
                m.Text,
                m.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                m.Comments.Count,
                followedIds.Contains(m.AuthorId)  // <- again: true only if you follow this author
            ))
            .ToList();
    }

    public async Task CreateCheepAsync(string authorName, string text)
    {
        // find or create author
        var author = await _db.Authors.FirstOrDefaultAsync(u => u.Name == authorName);
        if (author is null)
        {
            author = new Author { Name = authorName };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();
        }

        _db.Cheeps.Add(new Cheep
        {
            Text = text,
            TimeStamp = DateTime.UtcNow,
            AuthorId = author.AuthorId
        });

        await _db.SaveChangesAsync();
    }
    
    public List<CheepViewModel> GetCheepsFromFollowing(string currentAuthor, int page = 0, int pageSize = 32)
    {
        var followedIds = GetFollowedIds(currentAuthor);
        if (followedIds.Count == 0)
            return new List<CheepViewModel>();

        return _db.Cheeps
            .AsNoTracking()
            .Include(m => m.Author)
            .Include(m => m.Comments)
            .Where(m => followedIds.Contains(m.AuthorId))
            .OrderByDescending(m => m.TimeStamp)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(m => new CheepViewModel(
                m.CheepId,
                m.Author.Name,
                m.Text,
                m.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                m.Comments.Count,
                true  // by definition: in this feed you only see people you follow
            ))
            .ToList();
    }
}
