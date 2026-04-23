using System.Security.Claims;
using Bitstore.Application.Abstractions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Enums;
using Bitstore.Core.Models;
using Bitstore.DTO.Beat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace Bitstore.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BeatController(IBeatService beatService,
    IUserService userService, ICurrentUserService currentUserService): Controller
{
    private readonly IBeatService _beatService = beatService;
    private readonly IUserService _userService = userService;
    
    [Authorize(Roles = "Admin")]
    [HttpGet("allbeats")]
    public async Task<ActionResult<List<Beat>>> GetAllBeats()
    {
        var beats = await _beatService.GetBeats();
        return Ok(beats);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<BeatResponse>>> GetBeatsByUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty");
        
        var beats = await _beatService.GetBeatsByUser(userId);
        return Ok(beats);
    }
    
    [HttpPost("addbeat")]
    public async Task<IActionResult> CreateBeat(BeatRequest request)
    {
        await _beatService.CreateBeat(request);
        return Ok();
    }
}