using BaseRepository.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BaseRepository.Infrastructure.IntegrationTests.TestSupport;

public class TestDbContext : BaseDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
}
