using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Service;



public class CheepService : ICheepService
{
    private readonly ChatDBContext _db;

    public CheepService(ChatDBContext db) => _db = db;

    public List<CheepViewModel> GetCheeps(int page = 0, int pageSize = 32)
        => _db.Cheeps
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
                    m.Comments.Count))
              .ToList();

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 0, int pageSize = 32)
        => _db.Cheeps
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
                    m.Comments.Count))
              .ToList();

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
        // 1) Find the current author row
        var author = _db.Authors
            .AsNoTracking()
            .SingleOrDefault(a => a.Name == currentAuthor);

        if (author is null)
            return new List<CheepViewModel>();

        var currentAuthorId = author.AuthorId;

        // 2) All author IDs that current author follows
        var followedIds = _db.Followings
            .Where(f => f.FollowerId == currentAuthorId)
            .Select(f => f.FollowedId)
            .ToList();

        if (followedIds.Count == 0)
            return new List<CheepViewModel>();

        // 3) Cheeps from followed authors (only)
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
                m.Comments.Count))
            .ToList();
    }
}
