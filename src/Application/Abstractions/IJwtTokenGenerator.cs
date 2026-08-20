using System;
using BaseRepository.Domain.Entities;

namespace BaseRepository.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user);
}
