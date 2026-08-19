using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BaseRepository.Application.UnitTests.Messaging;

public class SenderTests
{
    private class Ping : IRequest<string>
    {
        public string Message { get; init; } = string.Empty;
    }

    private class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken)
            => Task.FromResult($"handled:{request.Message}");
    }

    private class TracingBehavior : IPipelineBehavior<Ping, string>
    {
        public static List<string> Log { get; } = new();

        public async Task<string> Handle(Ping request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            Log.Add("before");
            var result = await next();
            Log.Add("after");
            return result;
        }
    }

    [Fact]
    public async Task Send_DispatchesToTheRegisteredHandler()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<Ping, string>, PingHandler>();
        services.AddScoped<ISender, Sender>();
        var provider = services.BuildServiceProvider();

        var sender = provider.GetRequiredService<ISender>();
        var result = await sender.Send(new Ping { Message = "hi" });

        Assert.Equal("handled:hi", result);
    }

    [Fact]
    public async Task Send_RunsPipelineBehaviorsAroundTheHandler()
    {
        TracingBehavior.Log.Clear();
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<Ping, string>, PingHandler>();
        services.AddScoped<IPipelineBehavior<Ping, string>, TracingBehavior>();
        services.AddScoped<ISender, Sender>();
        var provider = services.BuildServiceProvider();

        var sender = provider.GetRequiredService<ISender>();
        await sender.Send(new Ping { Message = "hi" });

        Assert.Equal(new[] { "before", "after" }, TracingBehavior.Log);
    }
}
