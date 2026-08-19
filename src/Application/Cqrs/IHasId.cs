namespace BaseRepository.Application.Cqrs;

/// <summary>
/// Optional contract a response DTO implements so BaseCrudController can build a Location
/// header on Create. Without it, Create still returns 201, just without a Location header.
/// </summary>
public interface IHasId<TKey>
{
    TKey Id { get; }
}
