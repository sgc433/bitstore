using Bitstore.Application.Exceptions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Models;
using Bitstore.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Bitstore.DataAccess.Repositories;

public class UserRepository(BitstoreDbContext context): IUserRepository
{
    private readonly BitstoreDbContext _context = context;
    
    public async Task CreateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        
        Log.Information("Creating user with id {userId}...", user.Id);
        
        var userEntity = new UserEntity()
        {
            Id = user.Id,
            Username = user.Username,
            PasswordHash =  user.PasswordHash,
            Email = user.Email,
            Role = user.Role
        };
        
        await  _context.Users.AddAsync(userEntity);
        await _context.SaveChangesAsync();
        
    }

    public async Task<List<User>> GetAllAsync()
    {
        Log.Information("Getting all users...");
        var userEntities = await _context.Users
            .AsNoTracking()
            .ToListAsync();
        var users = userEntities
            .Select(u => User.Create(u.Id, u.Username,
                 u.Email, u.PasswordHash, u.Role))
            .ToList();
        return users;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var result = await _context.Users.AnyAsync(u => u.Email == email);
        return result;
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        Log.Information("Getting user by email {Email}", email);
        
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email);
        
        if (userEntity == null)
            throw new NotFoundException("User not found");
        
        var user = User.Create(userEntity.Id, userEntity.Username,userEntity.Email,
            userEntity.PasswordHash,  userEntity.Role);
        
        return user;
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        Log.Information("Getting user by id {UserId}", id);
        
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        
        if (userEntity == null)
            throw new NotFoundException($"User with id {id} not found");
        
        var user = User.Create(userEntity.Id, userEntity.Username,userEntity.Email,
            userEntity.PasswordHash, userEntity.Role);
        
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        Log.Information("Updating user with id {userId}...", user.Id);
        await _context.Users
            .Where(u => u.Id == user.Id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(userEntity => userEntity.Username, user.Username)
                .SetProperty(userEntity => userEntity.Email, user.Email)
                .SetProperty(userEntity => userEntity.Role, user.Role)
                .SetProperty(userEntity => userEntity.Balance, user.Balance));
        
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        Log.Warning("Deleting user with id {userId}...", userId);
        var result = await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteDeleteAsync();
        if (result == 0)
            throw new NotFoundException($"User with id {userId} not found");
        
        return true;
    }

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        Log.Information("Getting balance for user {userId} ...", userId);
        var userEntity = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new {u.Balance})
            .FirstOrDefaultAsync();
        
        if (userEntity == null)
            throw new NotFoundException($"User with id {userId} not found");
        return userEntity.Balance;
    }

    public async Task UpdateBalanceAsync(Guid userId, decimal amount)
    {
        Log.Information("Updating balance for user {userId} ...", userId);
        var updatedCount = await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(userEntity => userEntity.Balance, amount));
        
        if (updatedCount == 0)
            throw new NotFoundException($"User with id {userId} not found");
    }
}