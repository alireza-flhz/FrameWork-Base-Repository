using System;
using BaseRepository.Application.Abstractions;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.UnitTests.TestSupport;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user)
        => ($"token-for-{user.Id}", DateTimeOffset.UtcNow.AddHours(1));
}
