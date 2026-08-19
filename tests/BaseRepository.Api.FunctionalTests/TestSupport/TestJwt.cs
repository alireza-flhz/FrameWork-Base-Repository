using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BaseRepository.Api.FunctionalTests.TestSupport;

/// <summary>
/// Mints JWTs signed with a fixed test-only key, matching the Jwt:Issuer/Audience/SigningKey
/// configuration SampleWebApplicationFactory feeds into the app under test.
/// </summary>
public static class TestJwt
{
    public const string Issuer = "test-issuer";
    public const string Audience = "test-audience";
    public const string SigningKey = "this-is-a-test-only-signing-key-at-least-32-bytes-long";

    public static string CreateToken(string subject = "test-user")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: new List<Claim> { new(ClaimTypes.NameIdentifier, subject) },
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
