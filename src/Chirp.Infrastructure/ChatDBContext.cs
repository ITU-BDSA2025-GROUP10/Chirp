using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class ChatDBContext : DbContext
{
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }

    public ChatDBContext(DbContextOptions<ChatDBContext> options)
        : base(options)
    {
    }
}
