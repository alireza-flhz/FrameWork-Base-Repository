using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BaseRepository.Api.FunctionalTests.TestSupport;
using Xunit;

namespace BaseRepository.Api.FunctionalTests;

public class ApiVersioningTests : IClassFixture<SampleWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiVersioningTests(SampleWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken());
    }

    [Fact]
    public async Task V1Route_IsReachable()
    {
        var response = await _client.GetAsync("/api/v1/samples");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnsupportedVersion_DoesNotReach200()
    {
        var response = await _client.GetAsync("/api/v99/samples");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
