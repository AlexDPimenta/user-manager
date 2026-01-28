using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using UserManager.WebApi.Data;
using UserManager.WebApi.DTOs;
using UserManager.WebApi.Models;
using UserManager.WebApi.Services;

namespace UserManager.UnitTests;

public class UserServiceTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IConfiguration _configurationMock;
    private readonly UserService _sut;
    private readonly Faker _faker;

    public UserServiceTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _configurationMock = Substitute.For<IConfiguration>();
        _sut = new UserService(_userRepositoryMock, _configurationMock);
        _faker = new Faker();
    }

    [Fact]
    public async Task RegisterAsync_WhenUserAlreadyExists_ShouldThrowException()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = _faker.Internet.UserName(),
            Email = _faker.Internet.Email(),
            Password = "Password123!",
            FullName = _faker.Name.FullName()
        };

        _userRepositoryMock.ExistsByEmailAsync(registerDto.Email).Returns(true);

        // Act
        Func<Task> act = async () => await _sut.RegisterAsync(registerDto);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User already exists");
        
        await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>());
        await _userRepositoryMock.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterAsync_WhenDataIsValid_ShouldReturnUserDtoAndSaveUser()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = _faker.Internet.UserName(),
            Email = _faker.Internet.Email(),
            Password = "Password123!",
            FullName = _faker.Name.FullName()
        };

        _userRepositoryMock.ExistsByEmailAsync(registerDto.Email).Returns(false);

        // Act
        var result = await _sut.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(registerDto.Username);
        result.Email.Should().Be(registerDto.Email);
        result.FullName.Should().Be(registerDto.FullName);

        await _userRepositoryMock.Received(1).AddAsync(Arg.Is<User>(u => 
            u.Username == registerDto.Username && 
            u.Email == registerDto.Email &&
            u.FullName == registerDto.FullName &&
            !string.IsNullOrEmpty(u.PasswordHash) &&
            u.PasswordHash != registerDto.Password // Verify hashing happened
        ));
        await _userRepositoryMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnUserDto()
    {
        // Arrange
        var password = "Password123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Username = _faker.Internet.UserName(),
            Email = _faker.Internet.Email(),
            PasswordHash = passwordHash,
            FullName = _faker.Name.FullName()
        };

        var loginDto = new LoginDto
        {
            Email = user.Email,
            Password = password
        };

        _userRepositoryMock.GetByEmailAsync(loginDto.Email).Returns(user);

        // Act
        var result = await _sut.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = _faker.Internet.Email(),
            Password = "AnyPassword"
        };

        _userRepositoryMock.GetByEmailAsync(loginDto.Email).Returns((User?)null);

        // Act
        var result = await _sut.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var user = new User
        {
            Email = _faker.Internet.Email(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
        };

        var loginDto = new LoginDto
        {
            Email = user.Email,
            Password = "WrongPassword"
        };

        _userRepositoryMock.GetByEmailAsync(loginDto.Email).Returns(user);

        // Act
        var result = await _sut.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }
}
