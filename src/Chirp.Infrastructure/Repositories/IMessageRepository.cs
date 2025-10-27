using Chirp.Core.Models;

namespace Chirp.Infrastructure.Repositories;


// Keep a simple, slide-friendly shape.
// Methods exist but we won’t implement them yet.
public interface IMessageRepository
{
    Task<List<MessageDTO>> ReadMessagesAsync(string? author = null, int page = 0, int pageSize = 32);
    Task<int> CreateMessageAsync(MessageDTO message);
    Task UpdateMessageAsync(MessageDTO message);
    Task DeleteMessageAsync(int id);
}

