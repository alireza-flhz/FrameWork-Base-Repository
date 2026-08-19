using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BaseRepository.Api.FunctionalTests.TestSupport;
using Xunit;

namespace BaseRepository.Api.FunctionalTests;

public class OutputCachingTests : IClassFixture<SampleWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OutputCachingTests(SampleWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task PublicCachedEndpoint_ReturnsTheSameValueAcrossRapidCalls()
    {
        var first = await _client.GetAsync("/api/public/server-time");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstBody = await first.Content.ReadFromJsonAsync<TimeResponse>(JsonOptions);

        var second = await _client.GetAsync("/api/public/server-time");
        var secondBody = await second.Content.ReadFromJsonAsync<TimeResponse>(JsonOptions);

        Assert.NotEqual(0, firstBody!.ServerTimeTicks);
        Assert.Equal(firstBody.ServerTimeTicks, secondBody!.ServerTimeTicks);
    }

    private sealed class TimeResponse
    {
        public long ServerTimeTicks { get; set; }
    }
}
