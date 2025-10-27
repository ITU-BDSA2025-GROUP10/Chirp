namespace Chirp.Razor.Models;

public class User
{
    public int UserId { get; set; }            // PK (EF recognizes *Id)
    public string Name { get; set; } = null!;

    public string? DisplayName { get; set; }   // NEW for Step 6
    public string? Email { get; set; }          // optional, you can add later with a migration

    // Navigation
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}