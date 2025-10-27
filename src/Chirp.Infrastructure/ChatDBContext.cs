using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class ChatDBContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<User> Users { get; set; }

    public ChatDBContext(DbContextOptions<ChatDBContext> options)
        : base(options)
    {
    }
}
