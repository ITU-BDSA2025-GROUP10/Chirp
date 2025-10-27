namespace Chirp.Core.Models;

// Dummy DTO used for transferring data between layers.
// Keep it simple for now — you can change it later.
public class CheepDTO
{
    public int Id { get; set; }              // optional placeholder
    public string Author { get; set; } = "";
    public string Text { get; set; } = "";
    public string? Timestamp { get; set; }   // e.g., formatted string
}
