using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BaseRepository.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    public UnitOfWork(DbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
