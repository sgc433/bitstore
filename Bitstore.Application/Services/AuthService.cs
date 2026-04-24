using Bitstore.Application.Abstractions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Enums;
using Bitstore.Core.Models;
using Bitstore.DTO.Auth;
using Serilog;

namespace Bitstore.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider)
    : IAuthService
{
    public async Task Resgister(RegisterUserRequest request)
    {
        var existingUser = await userRepository.ExistsByEmailAsync(request.Email);
        
        if (existingUser)
            throw new Exception("User already exists");
        
        var hashedPassword = passwordHasher.Generate(request.Password);
        
        var user = User.Create(Guid.NewGuid(), request.Username, request.Email,
            hashedPassword, "Customer");
        
        Log.Information("Registering user with username {Username}", user.Username);
        
        await userRepository.CreateAsync(user);
    }

    public async Task<string> Login(LoginUserRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        
        var result = passwordHasher.Verify(request.Password, user.PasswordHash);
        
        if (!result)
            throw new Exception("Invalid username or password");

        var token = jwtProvider.GenerateToken(user);
        
        Log.Information("Login for user {Username}", user.Username);
        
        return token;

    }
}

