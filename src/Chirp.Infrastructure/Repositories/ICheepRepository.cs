using Chirp.Core.Models;

namespace Chirp.Infrastructure.Repositories;


// Keep a simple, slide-friendly shape.
// Methods exist but we won’t implement them yet.
public interface ICheepRepository
{
    Task<List<CheepDTO>> ReadCheepsAsync(string? author = null, int page = 0, int pageSize = 32);
    Task<List<CheepDTO>> ReadCheepsFromAuthorIdsAsync(List<int> authorIds, int page = 0, int pageSize = 32);
    Task<int> CreateCheepAsync(CheepDTO Cheep);
    Task UpdateCheepAsync(CheepDTO Cheep);
    Task DeleteCheepAsync(int id);
}

