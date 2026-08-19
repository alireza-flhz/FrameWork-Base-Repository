using System.Threading;
using System.Threading.Tasks;

namespace BaseRepository.Application.Messaging;

public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
