namespace Bitstore.Application.Abstractions;

public interface ICurrentUserService
{
    Guid GetUserId();
    string GetUserEmail();
    string GetUserRole();
    bool IsAuthenticated();
    bool IsInRole(string role);
    
}