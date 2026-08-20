using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using BaseRepository.Application.Auth;
using BaseRepository.Application.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseRepository.Api.Controllers;

/// <summary>
/// Part of the base, not the TodoItem sample - keep this alongside RegisterCommand/
/// LoginCommand/User even after you've deleted the TodoItem example.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
