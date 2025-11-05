using Chirp.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class ChatDBContext : IdentityDbContext<ApplicationAuthor>
{
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author?> Authors { get; set; }

    public ChatDBContext(DbContextOptions<ChatDBContext> options)
        : base(options)
    {
        
    }
}
