using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Infrastructure;

public record CheepViewModel(string Author, string Message, string Timestamp);
public interface IChatService
{

    List<CheepViewModel> GetCheeps(int page = 0, int pageSize = 32);
    List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 0, int pageSize = 32);
    Task CreateCheepAsync(string authorName, string text);
}
