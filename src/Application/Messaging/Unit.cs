namespace BaseRepository.Application.Messaging;

/// <summary>
/// Stand-in response type for a request that produces no meaningful value
/// (e.g. a delete command), since <see cref="IRequest{TResponse}"/> always needs one.
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = default;
}
