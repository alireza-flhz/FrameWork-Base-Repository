using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using BaseRepository.Domain.Entities;
using BaseRepository.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BaseRepository.Infrastructure.IntegrationTests.Security;

public class JwtTokenGeneratorTests
{
    private static JwtTokenGenerator CreateGenerator(string? signingKey = "a-test-signing-key-at-least-32-bytes-long")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:SigningKey"] = signingKey
            })
            .Build();

        return new JwtTokenGenerator(configuration);
    }

    private static User CreateUser(int id, string email)
    {
        var user = new User { Email = email, PasswordHash = "irrelevant" };
        typeof(BaseEntity<int>).GetProperty(nameof(BaseEntity<int>.Id))!.SetValue(user, id);
        return user;
    }

    [Fact]
    public void GenerateToken_ProducesATokenCarryingTheUsersIdAndEmail()
    {
        var generator = CreateGenerator();
        var user = CreateUser(42, "user@example.com");

        var (token, expiresAt) = generator.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
        Assert.Equal("42", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("user@example.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.True(expiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void GenerateToken_WithNoSigningKeyConfigured_ThrowsInvalidOperationException()
    {
        var generator = CreateGenerator(signingKey: "");
        var user = CreateUser(1, "user@example.com");

        Assert.Throws<InvalidOperationException>(() => generator.GenerateToken(user));
    }
}
