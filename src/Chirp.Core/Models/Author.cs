namespace Chirp.Core.Models;

public class Author
{
    public int AuthorId { get; set; }            // PK (EF recognizes *Id)
    public string Name { get; set; } = null!;

    public string? DisplayName { get; set; }   // NEW for Step 6
    public string? Email { get; set; }          // optional, you can add later with a migration
    
    public List<Author> Following { get; set; } = null!; // List of which authors is the user following
    
    public List<Author> Followers { get; set; } = null!; // List of who is following the user


    // Navigation
    public ICollection<Cheep> Cheeps { get; set; } = new List<Cheep>();
}
