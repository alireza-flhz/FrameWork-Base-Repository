using System.Threading;
using System.Threading.Tasks;

namespace BaseRepository.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
