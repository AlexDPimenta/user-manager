using Microsoft.AspNetCore.Mvc;
using UserManager.WebApi.DTOs;
using UserManager.WebApi.Services;

namespace UserManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationDto registration)
    {
        var result = await userService.RegisterAsync(registration);
        return result != null ? Ok(result) : BadRequest("Usuário já existe");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto login)
    {
        var token = await userService.LoginAsync(login);
        return token != null ? Ok(new { Token = token }) : Unauthorized("Credenciais inválidas");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await userService.GetByIdAsync(id);
        return user != null ? Ok(user) : NotFound();
    }
}

