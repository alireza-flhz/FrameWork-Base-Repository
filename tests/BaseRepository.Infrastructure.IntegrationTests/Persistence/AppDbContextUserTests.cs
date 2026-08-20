using System;
using System.Threading.Tasks;
using BaseRepository.Domain.Entities;
using BaseRepository.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseRepository.Infrastructure.IntegrationTests.Persistence;

/// <summary>
/// Proves AppDbContext's Email unique index (configured in its OnModelCreating override) is
/// actually enforced at the database level, as a safety net behind RegisterCommandHandler's
/// own AnyAsync check for the same thing.
/// </summary>
public class AppDbContextUserTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;

    public AppDbContextUserTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task SavingTwoUsers_WithTheSameEmail_ThrowsBecauseOfTheUniqueIndex()
    {
        _context.Users.Add(new User { Email = "dup@example.com", PasswordHash = "x" });
        await _context.SaveChangesAsync();

        _context.Users.Add(new User { Email = "dup@example.com", PasswordHash = "y" });

        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task SavingTwoUsers_WithDifferentEmails_Succeeds()
    {
        _context.Users.Add(new User { Email = "one@example.com", PasswordHash = "x" });
        _context.Users.Add(new User { Email = "two@example.com", PasswordHash = "y" });

        await _context.SaveChangesAsync();

        Assert.Equal(2, await _context.Users.CountAsync());
    }
}
