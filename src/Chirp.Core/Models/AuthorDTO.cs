namespace Chirp.Core.Models;

public class AuthorDto
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }

    public ICollection<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
}
