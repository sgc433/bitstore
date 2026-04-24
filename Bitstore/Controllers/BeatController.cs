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
public class BeatController(IBeatService beatService): Controller
{
    private readonly IBeatService _beatService = beatService;

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<List<BeatResponse>>> GetAllBeats()
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
    
    [HttpGet("{beatId}")]
    public async Task<ActionResult<BeatResponse>> GetBeatById(Guid beatId)
    {
        if (beatId == Guid.Empty)
            throw new ArgumentException("Beat id cannot be empty");

        var beat = await _beatService.GetBeatById(beatId);
        return Ok(beat);
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateBeat(BeatRequest request)
    {
        await _beatService.CreateBeat(request);
        return Ok();
    }
    
    [HttpPut("updatebeat/{beatId}")]
    public async Task<IActionResult> UpdateBeat(Guid beatId, UpdateBeatRequest request)
    {
        if (beatId == Guid.Empty)
            throw new ArgumentException("Beat id cannot be empty");

        await _beatService.UpdateBeat(beatId, request);
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("deletebeat/{beatId}")]
    public async Task<IActionResult> DeleteBeat(Guid beatId)
    {
        if (beatId == Guid.Empty)
            throw new ArgumentException("Beat id cannot be empty");

        var result = await _beatService.DeleteBeat(beatId);
        return Ok(result);
    }
    
    
    
}