using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BaseRepository.Application.Messaging;

internal abstract class RequestHandlerBase
{
    public abstract Task<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerBase
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;

        Task<TResponse> Handler() =>
            serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>().Handle(typedRequest, cancellationToken);

        var pipeline = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate(
                (RequestHandlerDelegate<TResponse>)Handler,
                (next, behavior) => (RequestHandlerDelegate<TResponse>)(() => behavior.Handle(typedRequest, next, cancellationToken)));

        return await pipeline();
    }
}
