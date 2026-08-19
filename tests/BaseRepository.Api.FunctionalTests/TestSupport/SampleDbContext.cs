using BaseRepository.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BaseRepository.Api.FunctionalTests.TestSupport;

public class SampleDbContext : BaseDbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options)
    {
    }

    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();
}
