using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BaseRepository.Api.FunctionalTests.TestSupport;
using BaseRepository.Application.Auth;
using Xunit;

namespace BaseRepository.Api.FunctionalTests;

/// <summary>
/// Proves Auth - part of the base, not the TodoItem sample - actually works end to end
/// through the real, unmodified Program.cs: register, login, and using the resulting token
/// against a real protected endpoint.
/// </summary>
public class AuthEndToEndTests : IClassFixture<RealAppWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndToEndTests(RealAppWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object RegisterPayload(string email, string password = "correct-horse") => new { Email = email, Password = password };

    [Fact]
    public async Task Register_WithANewEmail_Returns201WithAWorkingToken()
    {
        var email = $"{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", RegisterPayload(email));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        Assert.NotNull(result);
        Assert.Equal(email, result!.Email);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);

        // The issued token should actually be accepted by a protected endpoint, not just look
        // like a token.
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        var protectedResponse = await _client.GetAsync("/api/v1/todo-items");
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithAnEmailAlreadyTaken_Returns409()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", RegisterPayload(email));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", RegisterPayload(email));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithATooShortPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            RegisterPayload($"{Guid.NewGuid():N}@example.com", password: "short"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithTheCorrectPassword_Returns200WithAToken()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", RegisterPayload(email, "correct-horse"));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", RegisterPayload(email, "correct-horse"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        Assert.False(string.IsNullOrEmpty(result!.Token));
    }

    [Fact]
    public async Task Login_WithTheWrongPassword_Returns401()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", RegisterPayload(email, "correct-horse"));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", RegisterPayload(email, "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithAnUnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            RegisterPayload($"{Guid.NewGuid():N}@example.com"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
