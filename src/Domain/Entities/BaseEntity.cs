namespace BaseRepository.Domain.Entities;

public abstract class BaseEntity<TKey>
{
    public TKey Id { get; protected set; } = default!;
}
