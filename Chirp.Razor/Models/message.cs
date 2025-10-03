namespace Chirp.Razor.Models;

public class Message
{
    public int MessageId { get; set; }          // PK
    public string Text { get; set; } = null!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    // FK + navigation to User (author)
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}