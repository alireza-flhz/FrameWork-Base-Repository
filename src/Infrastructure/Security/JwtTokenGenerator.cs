using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BaseRepository.Application.Abstractions;
using BaseRepository.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BaseRepository.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user)
    {
        // Read lazily (not into a field/variable captured before this method runs) for the
        // same reason Program.cs's AddJwtBearer options delegate does - see its comment.
        var jwtSection = _configuration.GetSection("Jwt");
        var signingKey = jwtSection["SigningKey"];

        if (string.IsNullOrEmpty(signingKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured - set it before issuing tokens.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"] ?? "BaseRepository.Api",
            audience: jwtSection["Audience"] ?? "BaseRepository.Api",
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
