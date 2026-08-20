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
/// Proves a signed-in user can set/change their own phone number through the real,
/// unmodified Program.cs, and that nobody else's profile can be touched this way.
/// </summary>
public class ProfileEndToEndTests : IClassFixture<RealAppWebApplicationFactory>
{
    private readonly RealAppWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProfileEndToEndTests(RealAppWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync(string? email = null)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { Email = email ?? $"{Guid.NewGuid():N}@example.com", Password = "correct-horse" });

        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        return result!.Token;
    }

    [Fact]
    public async Task UpdatePhoneNumber_WithoutAToken_Returns401()
    {
        var response = await _client.PutAsJsonAsync("/api/v1/auth/me/phone", new { PhoneNumber = "09123456789" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WithAValidNumber_Returns200AndPersistsIt()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync("/api/v1/auth/me/phone", new { PhoneNumber = "+989123456789" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.Equal("09123456789", result!.PhoneNumber);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WithAnInvalidNumber_Returns400()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync("/api/v1/auth/me/phone", new { PhoneNumber = "not-a-number" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePhoneNumber_AlreadyTakenByAnotherUser_Returns409()
    {
        var firstUserToken = await RegisterAndGetTokenAsync();
        var firstClient = _factory.CreateClient();
        firstClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstUserToken);
        await firstClient.PutAsJsonAsync("/api/v1/auth/me/phone", new { PhoneNumber = "09121234567" });

        var secondUserToken = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondUserToken);

        var response = await _client.PutAsJsonAsync("/api/v1/auth/me/phone", new { PhoneNumber = "09121234567" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WithAnEmptyValue_ClearsIt()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PutAsJsonAsync("/api/v1/auth/me/phone", new { PhoneNumber = "09123456789" });

        var response = await _client.PutAsJsonAsync("/api/v1/auth/me/phone", new { PhoneNumber = (string?)null });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.Null(result!.PhoneNumber);
    }
}
