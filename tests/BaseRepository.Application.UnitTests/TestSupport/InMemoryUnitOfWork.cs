using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;

namespace BaseRepository.Application.UnitTests.TestSupport;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}
