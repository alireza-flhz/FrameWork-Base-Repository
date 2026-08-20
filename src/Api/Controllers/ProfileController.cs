using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using BaseRepository.Application.Auth;
using BaseRepository.Application.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseRepository.Api.Controllers;

/// <summary>
/// The signed-in user's own profile. Part of the base, not the TodoItem sample - unlike
/// AuthController, this one is authenticated: the current user comes from the token's claims
/// (ICurrentUser), never from a route/body parameter, so nobody can edit someone else's profile.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth/me")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ISender _sender;

    public ProfileController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPut("phone")]
    public async Task<ActionResult<UserProfileDto>> UpdatePhoneNumber([FromBody] UpdatePhoneNumberCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
