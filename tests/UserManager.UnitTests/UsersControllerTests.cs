using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using UserManager.WebApi.Controllers;
using UserManager.WebApi.DTOs;
using UserManager.WebApi.Services;

namespace UserManager.UnitTests;

public class UsersControllerTests
{
    private readonly IUserService _userServiceMock;
    private readonly UsersController _sut;
    private readonly Faker _faker;

    public UsersControllerTests()
    {
        _userServiceMock = Substitute.For<IUserService>();
        _sut = new UsersController(_userServiceMock);
        _faker = new Faker();
    }

    [Fact]
    public async Task Register_WhenSuccessful_ShouldReturnOk()
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

        var responseDto = new UserResponseDto(
            1,
            registrationDto.Username,
            registrationDto.FirstName,
            registrationDto.LastName,
            registrationDto.Weight,
            registrationDto.Address
        );

        _userServiceMock.RegisterAsync(registrationDto).Returns(responseDto);

        // Act
        var result = await _sut.Register(registrationDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(responseDto, options => options
                .Including(x => x.Id)
                .Including(x => x.Username)
                .Including(x => x.FirstName)
                .Including(x => x.LastName)
                .Including(x => x.Weight)
                .Including(x => x.Address));
    }

    [Fact]
    public async Task Register_WhenUserAlreadyExists_ShouldReturnBadRequest()
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

        // O controller atual retorna BadRequest se o resultado for null
        _userServiceMock.RegisterAsync(registrationDto).Returns((UserResponseDto?)null);

        // Act
        var result = await _sut.Register(registrationDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Usuário já existe");
    }

    [Fact]
    public async Task Login_WhenSuccessful_ShouldReturnOkWithToken()
    {
        // Arrange
        var loginDto = new UserLoginDto(_faker.Internet.UserName(), "Password123!");
        var token = "fake-jwt-token";

        _userServiceMock.LoginAsync(loginDto).Returns(token);

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { Token = token });
    }

    [Fact]
    public async Task Login_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto(_faker.Internet.UserName(), "WrongPassword");

        _userServiceMock.LoginAsync(loginDto).Returns((string?)null);

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Credenciais inválidas");
    }

    [Fact]
    public async Task GetById_WhenUserExists_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var responseDto = new UserResponseDto(
            userId,
            _faker.Internet.UserName(),
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            75.5,
            _faker.Address.FullAddress()
        );

        _userServiceMock.GetByIdAsync(userId).Returns(responseDto);

        // Act
        var result = await _sut.GetById(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(responseDto, options => options
                .Including(x => x.Id)
                .Including(x => x.Username)
                .Including(x => x.FirstName)
                .Including(x => x.LastName)
                .Including(x => x.Weight)
                .Including(x => x.Address));
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = 99;
        _userServiceMock.GetByIdAsync(userId).Returns((UserResponseDto?)null);

        // Act
        var result = await _sut.GetById(userId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
