using Bitstore.Application.Abstractions;
using Bitstore.Application.DTO.User;
using Bitstore.Application.Exceptions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Models;
using Serilog;

namespace Bitstore.Application.Services;

public class UserService(IUserRepository userRepository,
    ICurrentUserService currentUserService): IUserService
{
    public async Task<UserResponse> GetUserById(Guid userId)
    {
        
        if (currentUserService.GetUserRole() != "Admin")
            throw new UnauthorizedAccessException("Only admin users can do this");
        
        var user = await userRepository.GetByIdAsync(userId);

        var response = new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.Balance);
        
        Log.Information("Returning user by id {userId}", userId);
        
        return response;
    }

    public async Task<List<UserResponse>> GetAllUsers()
    {
        if (currentUserService.GetUserRole() != "Admin")
            throw new UnauthorizedAccessException("Only admin users can do this");
        
        var users = await userRepository.GetAllAsync();

        var response = users.Select(u =>
            new UserResponse(u.Id, u.Username,
                u.Email, u.Role, u.Balance)).ToList();
        
        Log.Information("Returning all users");
        return response;
    }

    public async Task<UserResponse> GetUserByEmail(string email)
    {
        if (currentUserService.GetUserRole() != "Admin")
            throw new UnauthorizedAccessException("Only admin users can do this");
        
        var user = await userRepository.GetByEmailAsync(email);
        
        var response = new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.Balance);
        
        Log.Information("Returning user by email {Email}", email);
        
        return response;
    }

    public async Task<bool> DeleteUserById(Guid userId)
    {
        if (currentUserService.GetUserRole() != "Admin")
            throw new UnauthorizedAccessException("Only admin users can do this");
        
        var result = await userRepository.DeleteAsync(userId);
        
        Log.Information("Deleting operation is {IsDeleted} ", result);
        
        return result;
    }

    public async Task UpdateUser(Guid userId, UserUpdateRequest request)
    {
        if (currentUserService.GetUserRole() != "Admin")
            throw new UnauthorizedAccessException("Only admin users can do this");
        
        var user = User.Create(
            userId,
            request.Username,
            request.Email,
            "",
            request.Role);
        
        Log.Information("User with id {UserId} was updated", userId);
        
        await  userRepository.UpdateAsync(user);
    }

    public async Task CreateUser(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        
        await userRepository.CreateAsync(user);
    }

    public async Task<decimal> GetBalance(Guid userId)
    {
        
        var currentUserId = currentUserService.GetUserId();
        var isAdmin = currentUserService.IsInRole("Admin");
        if (currentUserId != userId && !isAdmin)
        {
            Log.Warning("User {CurrentUserId} attempted to view balance of user {TargetUserId}"
            ,currentUserId, userId);
            throw new UnauthorizedAccessException("You can only view your own balance");
        }
                
        Log.Information("Balance requested for user {UserId}", userId);
        
        var balance = await userRepository.GetBalanceAsync(userId);
        return balance;
    }

    public async Task UpdateBalance(UserUpdateBalanceRequest request)
    {
        var currentUserId = currentUserService.GetUserId();
        var isAdmin = currentUserService.IsInRole("Admin");
        if (currentUserId != request.UserId && !isAdmin)
        {
            Log.Error("User {CurrentUserId} attempted to update balance of user {TargetUserId}"
                ,currentUserId, request.UserId);
            throw new UnauthorizedAccessException("Only admin users can update balance");
        }
        Log.Information("Balance of user {userId} was updated", request.UserId);
        
        await userRepository.UpdateBalanceAsync(request.UserId, request.Amount);
        
    }
}