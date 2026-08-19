using System;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Infrastructure.IntegrationTests.TestSupport;

public class TestEntity : BaseEntity<int>, IAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
