using BaseRepository.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseRepository.Infrastructure.Persistence;

/// <summary>
/// The template's DbContext - add your own DbSets here as you add entities. Users is part of
/// the base (Application.Auth) and should stay; TodoItems is the sample and can go once you
/// no longer need it - see TodoItem's doc comment.
/// </summary>
public class AppDbContext : BaseDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
