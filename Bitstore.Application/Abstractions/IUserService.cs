using Bitstore.Application.DTO.User;
using Bitstore.Core.Models;

namespace Bitstore.Application.Abstractions;

public interface IUserService
{
    Task<UserResponse> GetUserById(Guid userId);
    Task<List<UserResponse>> GetAllUsers();
    Task<UserResponse> GetUserByEmail(string email);
    Task<bool> DeleteUserById(Guid userId);
    Task UpdateUser(Guid userId, UserUpdateRequest request);
    Task CreateUser(User user);
    Task<decimal> GetBalance(Guid userId);
    Task UpdateBalance(UserUpdateBalanceRequest request);
}