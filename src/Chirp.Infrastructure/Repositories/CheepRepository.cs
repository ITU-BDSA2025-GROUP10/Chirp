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

    public async Task<List<CheepDTO>> ReadCheepsAsync(string? author = null, int page = 0, int pageSize = 32)
    {
        var q = _db.Cheeps
                   .AsNoTracking()
                   .Include(m => m.Author)
                   .Include(m => m.Comments)
                   .OrderByDescending(m => m.TimeStamp)
                   .AsQueryable();

        if (!string.IsNullOrWhiteSpace(author))
            q = q.Where(m => m.Author.Name == author);

        var items = await q.Skip(page * pageSize)
                           .Take(pageSize)
                           .Select(m => new CheepDTO
                                   {
                                       Id = m.CheepId,
                                       AuthorId = m.AuthorId,
                                       Author = m.Author.Name,
                                       Text = m.Text,
                                       Timestamp = m.TimeStamp.ToString("MM/dd/yy H:mm:ss", CultureInfo.InvariantCulture),
                                       CommentCount = m.Comments.Count
                                   })
                               .ToListAsync();

        return items;
    }

    public async Task<List<CheepDTO>> ReadCheepsFromAuthorIdsAsync(List<int> authorIds, int page = 0, int pageSize = 32)
    {
        if (authorIds == null || authorIds.Count == 0)
            return new List<CheepDTO>();

        var items = await _db.Cheeps
            .AsNoTracking()
            .Include(m => m.Author)
            .Include(m => m.Comments)
            .Where(m => authorIds.Contains(m.AuthorId))
            .OrderByDescending(m => m.TimeStamp)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(m => new CheepDTO
            {
                Id = m.CheepId,
                AuthorId = m.AuthorId,
                Author = m.Author.Name,
                Text = m.Text,
                Timestamp = m.TimeStamp.ToString("MM/dd/yy H:mm:ss", CultureInfo.InvariantCulture),
                CommentCount = m.Comments.Count
            })
            .ToListAsync();

        return items;
    }

    public async Task<int> CreateCheepAsync(CheepDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Author))
            throw new ArgumentException("Author is required.", nameof(dto.Author));
        if (string.IsNullOrWhiteSpace(dto.Text))
            throw new ArgumentException("Text is required.", nameof(dto.Text));

        var author = await _db.Authors.FirstOrDefaultAsync(u => u.Name == dto.Author);
        if (author is null)
        {
            author = new Author { Name = dto.Author };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();
        }

        var msg = new Cheep
        {
            Text = dto.Text,
            TimeStamp = DateTime.UtcNow,
            AuthorId = author.AuthorId
        };

        _db.Cheeps.Add(msg);
        await _db.SaveChangesAsync();

        return msg.CheepId;
    }

    public async Task UpdateCheepAsync(CheepDTO dto)
    {
        var msg = await _db.Cheeps.FirstOrDefaultAsync(m => m.CheepId == dto.Id);
        if (msg is null) return; // or throw KeyNotFoundException

        if (!string.IsNullOrWhiteSpace(dto.Text))
            msg.Text = dto.Text;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteCheepAsync(int id)
    {
        var msg = await _db.Cheeps.FirstOrDefaultAsync(m => m.CheepId == id);
        if (msg is null) return; // or throw KeyNotFoundException

        _db.Cheeps.Remove(msg);
        await _db.SaveChangesAsync();
    }
}
