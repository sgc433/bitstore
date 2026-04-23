using System.Security.Claims;
using Bitstore.Application.Abstractions;
using Bitstore.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Bitstore.Application.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    
    public Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("User not authenticated");
        
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        
        throw new InvalidOperationException($"Invalid user id claim: {userIdClaim}");
        //return Guid.Parse(userIdClaim);
    }
    
    public string GetUserEmail()
    {
        var userEmailClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        
        return userEmailClaim ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    public string GetUserRole()
    {
        var userRoleClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.Role) ??  string.Empty;
        
        return userRoleClaim ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}