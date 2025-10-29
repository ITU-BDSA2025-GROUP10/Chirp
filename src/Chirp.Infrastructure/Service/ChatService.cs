using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Service;



public class ChatService : IChatService
{
    private readonly ChatDBContext _db;

    public ChatService(ChatDBContext db) => _db = db;

    public List<CheepViewModel> GetCheeps(int page = 0, int pageSize = 32)
        => _db.Cheeps
              .AsNoTracking()
              .Include(m => m.Author)
              .OrderByDescending(m => m.TimeStamp)
              .Skip(page * pageSize)
              .Take(pageSize)
              .Select(m => new CheepViewModel(
                    m.Author.Name,
                    m.Text,
                    m.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
              .ToList();

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 0, int pageSize = 32)
        => _db.Cheeps
              .AsNoTracking()
              .Include(m => m.Author)
              .Where(m => m.Author.Name == author)
              .OrderByDescending(m => m.TimeStamp)
              .Skip(page * pageSize)
              .Take(pageSize)
              .Select(m => new CheepViewModel(
                    m.Author.Name,
                    m.Text,
                    m.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
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
}
