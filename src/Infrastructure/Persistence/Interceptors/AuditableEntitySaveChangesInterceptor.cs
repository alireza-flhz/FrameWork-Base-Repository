using System;
using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BaseRepository.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Stamps CreatedAt/LastModifiedAt on <see cref="IAuditableEntity"/> entities and turns a
/// staged delete of an <see cref="ISoftDelete"/> entity into an update (IsDeleted = true)
/// instead of an actual row deletion. Register via
/// <c>optionsBuilder.AddInterceptors(new AuditableEntitySaveChangesInterceptor())</c>.
/// </summary>
public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public AuditableEntitySaveChangesInterceptor(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateTrackedEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateTrackedEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateTrackedEntities(DbContext? context)
    {
        if (context is null)
            return;

        var utcNow = _timeProvider.GetUtcNow();

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.LastModifiedAt = utcNow;
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State != EntityState.Deleted)
                continue;

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = utcNow;
        }
    }
}
