using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BaseRepository.Api.FunctionalTests.TestSupport;
using BaseRepository.Application.TodoItems;
using Xunit;

namespace BaseRepository.Api.FunctionalTests;

/// <summary>
/// Proves the template's shipped example - TodoItem, wired through the real, unmodified
/// Program.cs - actually works, not just a test double of the same shape.
/// </summary>
public class TodoItemsEndToEndTests : IClassFixture<RealAppWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TodoItemsEndToEndTests(RealAppWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken());
    }

    [Fact]
    public async Task FullCrudLifecycle_WorksThroughTheUnmodifiedProgram()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/todo-items", new { Title = "buy milk" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemDto>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.False(created.IsDone);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/todo-items/{created.Id}",
            new { Title = "buy milk", IsDone = true });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TodoItemDto>();
        Assert.True(updated!.IsDone);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/todo-items/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await _client.GetAsync($"/api/v1/todo-items/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task Create_WithBlankTitle_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/todo-items", new { Title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
