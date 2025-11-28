using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Models;

public class Comment
{
    public int CommentId { get; set; }
    
    [StringLength(100)]
    public required string Text { get; set; }
    
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    
    //foreign Key
    public int CheepId { get; set; }
    public Cheep Cheep { get; set; } = null!;
    
    //foreign Key to Author
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}
