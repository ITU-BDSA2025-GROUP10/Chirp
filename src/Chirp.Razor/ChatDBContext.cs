using Microsoft.EntityFrameworkCore;
using Chirp.Razor.Models;

public class ChatDBContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<User> Users { get; set; }

    public ChatDBContext(DbContextOptions<ChatDBContext> options)
        : base(options)
    {
    }
}