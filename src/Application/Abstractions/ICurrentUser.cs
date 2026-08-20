namespace BaseRepository.Application.Abstractions;

/// <summary>
/// The authenticated caller, resolved from the current request's claims. Implemented in Api
/// (needs HttpContext) so Application stays free of any ASP.NET Core dependency.
/// </summary>
public interface ICurrentUser
{
    int? UserId { get; }
}
