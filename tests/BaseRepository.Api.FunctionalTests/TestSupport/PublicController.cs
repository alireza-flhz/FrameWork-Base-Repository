using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BaseRepository.Api.FunctionalTests.TestSupport;

/// <summary>
/// Deliberately anonymous + cached, to prove output caching works without the data-leak risk
/// of caching an [Authorize]-protected response (see BaseCrudController's remarks).
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public")]
public class PublicController : ControllerBase
{
    [HttpGet("server-time")]
    [OutputCache(Duration = 30)]
    public IActionResult GetServerTime() => Ok(new { serverTimeTicks = DateTime.UtcNow.Ticks });
}
