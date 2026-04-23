using Bitstore.Application.Abstractions;
using Bitstore.Core.Abstractions;
using Bitstore.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Bitstore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService): Controller
{
    private readonly IAuthService _authService = authService;
    
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserRequest request)
    {
        var token =  await _authService.Login(request);
        return Ok(token);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        await _authService.Resgister(request);
        return Ok();
    }
}