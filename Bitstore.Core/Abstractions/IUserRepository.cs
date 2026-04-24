using Bitstore.Core.Models;

namespace Bitstore.Core.Abstractions;

public interface IUserRepository
{
    Task CreateAsync(User user);
    Task<List<User>> GetAllAsync();
    Task<bool> ExistsByEmailAsync(string email);
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByIdAsync(Guid id);
    Task UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid userId);
    Task<decimal> GetBalanceAsync(Guid userId);
    Task UpdateBalanceAsync(Guid userId, decimal amount);
}