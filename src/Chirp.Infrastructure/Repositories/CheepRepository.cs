using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Chirp.Infrastructure.Repositories;

public class CheepRepository : ICheepRepository
{
    private readonly ChatDBContext _db;

    public CheepRepository(ChatDBContext db)
    {
        _db = db;
    }

    // READ (optionally filter by author) + paging
    public async Task<List<CheepDTO>> ReadCheepsAsync(string? author = null, int page = 0, int pageSize = 32)
    {
        var q = _db.Cheeps
                   .AsNoTracking()
                   .Include(m => m.User)
                   .OrderByDescending(m => m.TimeStamp)
                   .AsQueryable();

        if (!string.IsNullOrWhiteSpace(author))
            q = q.Where(m => m.User.Name == author);

        var items = await q.Skip(page * pageSize)
                           .Take(pageSize)
                           .Select(m => new CheepDTO
                           {
                               Id = m.CheepId,
                               Author = m.User.Name,
                               Text = m.Text,
                               Timestamp = m.TimeStamp.ToString("MM/dd/yy H:mm:ss", CultureInfo.InvariantCulture)})
                           .ToListAsync();

        return items;
    }

    // CREATE (find-or-create User, insert Cheep)
    public async Task<int> CreateCheepAsync(CheepDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Author))
            throw new ArgumentException("Author is required.", nameof(dto.Author));
        if (string.IsNullOrWhiteSpace(dto.Text))
            throw new ArgumentException("Text is required.", nameof(dto.Text));

        // find or create the author
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == dto.Author);
        if (user is null)
        {
            user = new User { Name = dto.Author };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        var msg = new Cheep
        {
            Text = dto.Text,
            TimeStamp = DateTime.UtcNow,
            UserId = user.UserId
        };

        _db.Cheeps.Add(msg);
        await _db.SaveChangesAsync();

        return msg.CheepId;
    }

    // UPDATE (update Cheep text; keep it simple for now)
    public async Task UpdateCheepAsync(CheepDTO dto)
    {
        var msg = await _db.Cheeps.FirstOrDefaultAsync(m => m.CheepId == dto.Id);
        if (msg is null) return; // or throw KeyNotFoundException

        // For the slides, just update text. (You can extend later.)
        if (!string.IsNullOrWhiteSpace(dto.Text))
            msg.Text = dto.Text;

        await _db.SaveChangesAsync();
    }

    // DELETE
    public async Task DeleteCheepAsync(int id)
    {
        var msg = await _db.Cheeps.FirstOrDefaultAsync(m => m.CheepId == id);
        if (msg is null) return; // or throw KeyNotFoundException

        _db.Cheeps.Remove(msg);
        await _db.SaveChangesAsync();
    }
}
