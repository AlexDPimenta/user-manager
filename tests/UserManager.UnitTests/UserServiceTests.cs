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
        var registrationDto = new UserRegistrationDto(
            _faker.Internet.UserName(),
            "Password123!",
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            _faker.Random.Double(50, 100),
            _faker.Address.FullAddress()
        );

        _userRepositoryMock.ExistsByUsernameAsync(registrationDto.Username).Returns(true);

        // Act
        Func<Task> act = async () => await _sut.RegisterAsync(registrationDto);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User already exists");
        
        await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>());
        await _userRepositoryMock.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterAsync_WhenDataIsValid_ShouldReturnUserResponseDtoAndSaveUser()
    {
        // Arrange
        var registrationDto = new UserRegistrationDto(
            _faker.Internet.UserName(),
            "Password123!",
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            _faker.Random.Double(50, 100),
            _faker.Address.FullAddress()
        );

        _userRepositoryMock.ExistsByUsernameAsync(registrationDto.Username).Returns(false);

        // Act
        var result = await _sut.RegisterAsync(registrationDto);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(registrationDto.Username);
        result.FirstName.Should().Be(registrationDto.FirstName);
        result.LastName.Should().Be(registrationDto.LastName);
        result.Weight.Should().Be(registrationDto.Weight);
        result.Address.Should().Be(registrationDto.Address);

        await _userRepositoryMock.Received(1).AddAsync(Arg.Is<User>(u => 
            u.Username == registrationDto.Username && 
            u.FirstName == registrationDto.FirstName &&
            u.LastName == registrationDto.LastName &&
            u.Weight == registrationDto.Weight &&
            u.Address == registrationDto.Address &&
            !string.IsNullOrEmpty(u.PasswordHash) &&
            u.PasswordHash != registrationDto.Password
        ));
        await _userRepositoryMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnToken()
    {
        // Arrange
        var password = "Password123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Username = _faker.Internet.UserName(),
            PasswordHash = passwordHash,
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };

        var loginDto = new UserLoginDto(user.Username, password);

        _userRepositoryMock.GetByUsernameAsync(loginDto.Username).Returns(user);

        // Act
        var result = await _sut.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("fake-jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var loginDto = new UserLoginDto(_faker.Internet.UserName(), "AnyPassword");

        _userRepositoryMock.GetByUsernameAsync(loginDto.Username).Returns((User?)null);

        // Act
        var result = await _sut.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUserResponseDto()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = _faker.Internet.UserName(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            Weight = 75.5,
            Address = _faker.Address.FullAddress()
        };

        _userRepositoryMock.GetByIdAsync(user.Id).Returns(user);

        // Act
        var result = await _sut.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Weight.Should().Be(user.Weight);
        result.Address.Should().Be(user.Address);
    }
}
