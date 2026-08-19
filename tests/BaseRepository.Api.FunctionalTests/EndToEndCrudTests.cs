using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BaseRepository.Api.FunctionalTests.TestSupport;
using BaseRepository.Domain.Common;
using Xunit;

namespace BaseRepository.Api.FunctionalTests;

public class EndToEndCrudTests : IClassFixture<SampleWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EndToEndCrudTests(SampleWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullCrudLifecycle_WorksEndToEnd()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/samples", new { Name = "first" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var created = await createResponse.Content.ReadFromJsonAsync<SampleDto>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("first", created.Name);

        var getResponse = await _client.GetAsync($"/api/samples/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<SampleDto>();
        Assert.Equal("first", fetched!.Name);

        var listResponse = await _client.GetAsync("/api/samples?pageIndex=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<SampleDto>>();
        Assert.NotNull(page);
        Assert.True(page!.TotalCount >= 1);

        var updateResponse = await _client.PutAsJsonAsync($"/api/samples/{created.Id}", new { Name = "changed" });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<SampleDto>();
        Assert.Equal("changed", updated!.Name);

        var deleteResponse = await _client.DeleteAsync($"/api/samples/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.GetAsync($"/api/samples/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
        Assert.Equal("application/problem+json", afterDeleteResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Create_WithInvalidDto_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/samples", new { Name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Name", body);
    }

    [Fact]
    public async Task GetById_ForMissingEntity_Returns404()
    {
        var response = await _client.GetAsync("/api/samples/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
