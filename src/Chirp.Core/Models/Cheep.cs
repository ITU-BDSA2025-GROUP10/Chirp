using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Models;

public class Cheep
{
    public int CheepId { get; set; }          // PK
    [StringLength(500)]
    public required string Text { get; set; } = null!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    // FK + navigation to User (author)
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
