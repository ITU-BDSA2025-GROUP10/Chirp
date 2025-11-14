using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Models;

public class Cheep
{
    public int CheepId { get; set; }          // PK
    [StringLength(100)]
    public required string Text { get; set; } = null!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    // FK + navigation to Author (author)
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}
