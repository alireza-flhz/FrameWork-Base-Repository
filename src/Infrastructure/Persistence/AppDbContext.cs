using BaseRepository.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseRepository.Infrastructure.Persistence;

/// <summary>
/// The template's example DbContext - add your own DbSets here as you add entities. Delete
/// TodoItems (and the rest of the TodoItem sample) once you no longer need the example.
/// </summary>
public class AppDbContext : BaseDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}
