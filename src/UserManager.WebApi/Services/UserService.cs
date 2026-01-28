using UserManager.WebApi.Data;
using UserManager.WebApi.DTOs;
using UserManager.WebApi.Models;

namespace UserManager.WebApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<UserResponseDto?> RegisterAsync(UserRegistrationDto registration)
    {
        if (await _userRepository.ExistsByUsernameAsync(registration.Username))
            throw new Exception("User already exists");

        var user = new User
        {
            Username = registration.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registration.Password),
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Weight = registration.Weight,
            Address = registration.Address
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return new UserResponseDto(
            user.Id,
            user.Username,
            user.FirstName,
            user.LastName,
            user.Weight,
            user.Address
        );
    }

    public async Task<string?> LoginAsync(UserLoginDto login)
    {
        var user = await _userRepository.GetByUsernameAsync(login.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            return null;

        return "fake-jwt-token"; 
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return new UserResponseDto(
            user.Id,
            user.Username,
            user.FirstName,
            user.LastName,
            user.Weight,
            user.Address
        );
    }
}
