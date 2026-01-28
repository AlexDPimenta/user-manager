using Microsoft.EntityFrameworkCore;
using UserManager.WebApi.Data;
using UserManager.WebApi.DTOs;
using UserManager.WebApi.Models;
using BC = BCrypt.Net.BCrypt;

namespace UserManager.WebApi.Services;

public class UserService(UserManagerDbContext context, IConfiguration configuration) : IUserService
{
    public async Task<UserResponseDto?> RegisterAsync(UserRegistrationDto registration)
    {
        if (await context.Users.AnyAsync(u => u.Username == registration.Username))
            return null;

        var user = new User
        {
            Username = registration.Username,
            PasswordHash = BC.HashPassword(registration.Password),
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Weight = registration.Weight,
            Address = registration.Address
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<string?> LoginAsync(UserLoginDto login)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
        if (user == null || !BC.Verify(login.Password, user.PasswordHash))
            return null;

        // Para fins de treino, vamos retornar um "token" fake ou apenas sucesso
        // Em um cenário real, aqui geramos o JWT
        return "fake-jwt-token-for-training";
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await context.Users.FindAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    private static UserResponseDto MapToDto(User user) =>
        new UserResponseDto(user.Id, user.Username, user.FirstName, user.LastName, user.Weight, user.Address);
}

