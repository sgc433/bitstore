using Bitstore.Application.Abstractions;
using Bitstore.Application.DTO.User;
using Bitstore.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bitstore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService): Controller
{
    private readonly IUserService _userService =  userService;
    
    [Authorize(Roles = "Admin")]
    [HttpGet("balance")]
    public async Task<ActionResult<decimal>> GetBalanceOfUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        
        var balance = await _userService.GetBalance(userId);
        return Ok(balance);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("userbyemail")]
    public async Task<ActionResult<UserResponse>> GetUserByEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));
        
        var response = await _userService.GetUserByEmail(email);
        return Ok(response);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("userbyid")]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException($"User id is required");
        
        var response = await _userService.GetUserById(userId);
        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("allusers")]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("updatebalance")]
    public async Task<IActionResult> UpdateBalance(UserUpdateBalanceRequest request)
    {
        await _userService.UpdateBalance(request);
        return Ok();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("updateuser")]
    public async Task<IActionResult> UpdateUserById(Guid userId, UserUpdateRequest request)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException($"User id is required");
        
        await _userService.UpdateUser(userId, request);
        return Ok();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("deleteuser")]
    public async Task<ActionResult<bool>> DeleteUserById(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException($"User id is required");
        
        var result = await _userService.DeleteUserById(userId);
        return Ok(result);
    }
    
    
}