namespace BaseRepository.Application.Auth;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
