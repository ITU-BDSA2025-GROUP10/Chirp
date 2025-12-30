namespace Chirp.Core.Models;

public class CheepDTO
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string Author { get; set; } = "";
    public string Text { get; set; } = "";
    public string? Timestamp { get; set; }

    public int CommentCount { get; set; }
}
