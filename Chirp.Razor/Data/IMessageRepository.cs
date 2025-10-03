using Chirp.Razor.Models;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Data;


// Keep a simple, slide-friendly shape.
// Methods exist but we won’t implement them yet.
public interface IMessageRepository
{
    Task<List<MessageDTO>> ReadMessagesAsync(string? author = null, int page = 0, int pageSize = 32);
    Task<int> CreateMessageAsync(MessageDTO message);
    Task UpdateMessageAsync(MessageDTO message);
    Task DeleteMessageAsync(int id);
}

public class MessageRepository : IMessageRepository
{
    private readonly ChatDBContext _db;

    public MessageRepository(ChatDBContext db)
    {
        _db = db;
    }

    // READ (optionally filter by author) + paging
    public async Task<List<MessageDTO>> ReadMessagesAsync(string? author = null, int page = 0, int pageSize = 32)
    {
        var q = _db.Messages
                   .AsNoTracking()
                   .Include(m => m.User)
                   .OrderByDescending(m => m.TimeStamp)
                   .AsQueryable();

        if (!string.IsNullOrWhiteSpace(author))
            q = q.Where(m => m.User.Name == author);

        var items = await q.Skip(page * pageSize)
                           .Take(pageSize)
                           .Select(m => new MessageDTO
                           {
                               Id = m.MessageId,
                               Author = m.User.Name,
                               Text = m.Text,
                               Timestamp = m.TimeStamp.ToString("MM/dd/yy H:mm:ss")
                           })
                           .ToListAsync();

        return items;
    }

    // CREATE (find-or-create User, insert Message)
    public async Task<int> CreateMessageAsync(MessageDTO dto)
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

        var msg = new Message
        {
            Text = dto.Text,
            TimeStamp = DateTime.UtcNow,
            UserId = user.UserId
        };

        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        return msg.MessageId;
    }

    // UPDATE (update message text; keep it simple for now)
    public async Task UpdateMessageAsync(MessageDTO dto)
    {
        var msg = await _db.Messages.FirstOrDefaultAsync(m => m.MessageId == dto.Id);
        if (msg is null) return; // or throw KeyNotFoundException

        // For the slides, just update text. (You can extend later.)
        if (!string.IsNullOrWhiteSpace(dto.Text))
            msg.Text = dto.Text;

        await _db.SaveChangesAsync();
    }

    // DELETE
    public async Task DeleteMessageAsync(int id)
    {
        var msg = await _db.Messages.FirstOrDefaultAsync(m => m.MessageId == id);
        if (msg is null) return; // or throw KeyNotFoundException

        _db.Messages.Remove(msg);
        await _db.SaveChangesAsync();
    }
}