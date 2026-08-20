using System;

namespace BaseRepository.Domain.Entities;

/// <summary>
/// Part of the reusable base, not the TodoItem sample - unlike TodoItem, keep this (and the
/// rest of Application.Auth/AuthController) even after you've deleted the sample.
/// </summary>
public class User : BaseEntity<int>, IAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
