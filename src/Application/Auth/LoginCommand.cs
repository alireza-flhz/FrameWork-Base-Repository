using BaseRepository.Application.Messaging;

namespace BaseRepository.Application.Auth;

public class LoginCommand : IRequest<AuthResultDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
