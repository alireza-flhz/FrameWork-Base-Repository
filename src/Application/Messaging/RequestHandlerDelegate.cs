using System.Threading.Tasks;

namespace BaseRepository.Application.Messaging;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
