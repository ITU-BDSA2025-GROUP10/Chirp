namespace Chirp.Core.Models;

public class CommentDTO
{
    public int CommentId { get; set; }
    public string Text { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public DateTime TimeStamp { get; set; }
    public int CheepId { get; set; }
    
}
