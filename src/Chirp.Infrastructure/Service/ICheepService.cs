namespace Chirp.Infrastructure.Service;

public record CheepViewModel(int Id, string Author, string Cheep, string Timestamp, int CommentCount);
public interface ICheepService
{

    List<CheepViewModel> GetCheeps(int page = 0, int pageSize = 32);
    List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 0, int pageSize = 32);
    Task CreateCheepAsync(string authorName, string text);
}
