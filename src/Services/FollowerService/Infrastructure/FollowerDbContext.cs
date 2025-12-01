using FollowerService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FollowerService.Infrastructure;

public class FollowerDbContext : DbContext
{
    public FollowerDbContext(DbContextOptions<FollowerDbContext> options) : base(options)
    {
    }

    public DbSet<Follow> Follows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Follow>()
            .HasIndex(f => new { f.UserId, f.ArtistId })
            .IsUnique();
    }
}
