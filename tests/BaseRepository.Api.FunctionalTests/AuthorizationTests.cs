using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BaseRepository.Api.FunctionalTests.TestSupport;
using Xunit;

namespace BaseRepository.Api.FunctionalTests;

public class AuthorizationTests : IClassFixture<SampleWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorizationTests(SampleWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetList_WithoutAToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/samples");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetList_WithAValidToken_Returns200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken());

        var response = await _client.GetAsync("/api/v1/samples");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetList_WithAGarbageToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

        var response = await _client.GetAsync("/api/v1/samples");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
