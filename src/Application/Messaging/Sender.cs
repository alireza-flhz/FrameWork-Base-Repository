using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BaseRepository.Application.Messaging;

public sealed class Sender : ISender
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerBase> Wrappers = new();

    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var wrapper = Wrappers.GetOrAdd(requestType, rt =>
        {
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(rt, typeof(TResponse));
            return (RequestHandlerBase)Activator.CreateInstance(wrapperType)!;
        });

        var result = await wrapper.Handle(request, _serviceProvider, cancellationToken);
        return (TResponse)result!;
    }
}
