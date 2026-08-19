using System;

namespace BaseRepository.Domain.Entities;

/// <summary>
/// Opt-in contract for entities that should never be hard-deleted. Infrastructure turns a
/// staged <c>Remove</c> on these entities into an update (IsDeleted = true) and applies a
/// global query filter so soft-deleted rows disappear from normal queries by default.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
