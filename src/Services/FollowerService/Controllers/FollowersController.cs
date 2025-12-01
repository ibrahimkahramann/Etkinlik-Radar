using FollowerService.Features.Follows.Commands.FollowArtist;
using FollowerService.Features.Follows.Queries.GetFollows;
using FollowerService.Features.Follows.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FollowerService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FollowersController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> FollowArtist([FromBody] FollowArtistRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new FollowArtistCommand(userId, request.ArtistId);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return Conflict("Already following this artist.");
        }

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetFollows()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = new GetFollowsQuery(userId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}

public record FollowArtistRequest(string ArtistId);
