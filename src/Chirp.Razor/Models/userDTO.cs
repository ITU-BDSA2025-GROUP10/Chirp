namespace Chirp.Razor.Models;

public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }

    public ICollection<MessageDTO> Messages { get; set; } = new List<MessageDTO>();
}