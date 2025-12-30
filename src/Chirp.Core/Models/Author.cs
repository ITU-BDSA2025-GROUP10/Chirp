using Microsoft.AspNetCore.Identity;
namespace Chirp.Core.Models;

public class Author : IdentityUser<int>
{
    
    public List<Following> Following { get; set; } = null!; 
    
    public List<Following> Followers { get; set; } = null!; 
    
    // Navigation
    public ICollection<Cheep> Cheeps { get; set; } = new List<Cheep>();
}
