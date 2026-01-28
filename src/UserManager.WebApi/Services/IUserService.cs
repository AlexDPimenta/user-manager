using UserManager.WebApi.DTOs;
using UserManager.WebApi.Models;

namespace UserManager.WebApi.Services;

public interface IUserService
{
    Task<UserResponseDto?> RegisterAsync(UserRegistrationDto registration);
    Task<string?> LoginAsync(UserLoginDto login);
    Task<UserResponseDto?> GetByIdAsync(int id);
}

