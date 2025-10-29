namespace Chirp.Core.Models;

public class Author
{
    public int AuthorId { get; set; }            // PK (EF recognizes *Id)
    public string Name { get; set; } = null!;

    public string? DisplayName { get; set; }   // NEW for Step 6
    public string? Email { get; set; }          // optional, you can add later with a migration

    // Navigation
    public ICollection<Cheep> Cheeps { get; set; } = new List<Cheep>();
}
