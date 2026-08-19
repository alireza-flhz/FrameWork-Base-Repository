using System;

namespace BaseRepository.Domain.Entities;

/// <summary>
/// Opt-in contract for entities whose CreatedAt/LastModifiedAt fields are stamped
/// automatically by <c>AuditableEntitySaveChangesInterceptor</c> (Infrastructure).
/// CreatedBy/LastModifiedBy stay unset until a current-user abstraction lands (Phase 4).
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}
