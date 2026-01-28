namespace UserManager.WebApi.DTOs;

public record UserRegistrationDto(
    string Username,
    string Password,
    string FirstName,
    string LastName,
    double Weight,
    string Address
);

public record UserLoginDto(
    string Username,
    string Password
);

public record UserResponseDto(
    int Id,
    string Username,
    string FirstName,
    string LastName,
    double Weight,
    string Address
);

