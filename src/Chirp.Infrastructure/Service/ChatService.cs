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
              .Include(m => m.User)
              .OrderByDescending(m => m.TimeStamp)
              .Skip(page * pageSize)
              .Take(pageSize)
              .Select(m => new CheepViewModel(
                    m.User.Name,
                    m.Text,
                    m.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
              .ToList();

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 0, int pageSize = 32)
        => _db.Cheeps
              .AsNoTracking()
              .Include(m => m.User)
              .Where(m => m.User.Name == author)
              .OrderByDescending(m => m.TimeStamp)
              .Skip(page * pageSize)
              .Take(pageSize)
              .Select(m => new CheepViewModel(
                    m.User.Name,
                    m.Text,
                    m.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
              .ToList();

    public async Task CreateCheepAsync(string authorName, string text)
    {
        // find or create author
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == authorName);
        if (user is null)
        {
            user = new User { Name = authorName };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        _db.Cheeps.Add(new Cheep
        {
            Text = text,
            TimeStamp = DateTime.UtcNow,
            UserId = user.UserId
        });

        await _db.SaveChangesAsync();
    }
}
