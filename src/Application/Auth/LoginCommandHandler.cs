using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Entities;
using BaseRepository.Domain.Exceptions;

namespace BaseRepository.Application.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IRepository<User, int> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IRepository<User, int> repository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _repository.FirstOrDefaultAsync(new UserByEmailSpecification(email), cancellationToken);

        // Same failure for "no such user" and "wrong password" - don't let a login attempt
        // reveal whether an email is registered.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new AuthenticationFailedException("Invalid email or password.");

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResultDto
        {
            UserId = user.Id,
            Email = user.Email,
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}
