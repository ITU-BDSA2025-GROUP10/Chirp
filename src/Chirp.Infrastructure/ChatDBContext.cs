using Chirp.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class ChatDBContext : IdentityDbContext<ApplicationAuthor>
{
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author?> Authors { get; set; }
    public DbSet<Following?> Followings { get; set; }
    

    public ChatDBContext(DbContextOptions<ChatDBContext> options)
        : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Following>()
            .HasKey(f => new { f.FollowerId, f.FollowedId });

        modelBuilder.Entity<Following>()
            .HasOne(f => f.Follower)
            .WithMany(a => a.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Following>()
            .HasOne(f => f.Followed)
            .WithMany(a => a.Followers)
            .HasForeignKey(f => f.FollowedId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
