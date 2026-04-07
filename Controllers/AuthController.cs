using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Andreev.Data;
using Andreev.Models;
using Andreev.Models.Auth;
using Andreev.Services;

namespace Andreev.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext context,
        IJwtTokenService tokenService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation($"Попытка регистрации: {request.Email}");

            var existingUser = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            _logger.LogInformation($"Сгенерирован хеш: {passwordHash}");

            var user = new AppUser
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Пользователь сохранен с ID: {user.Id}");

            var token = _tokenService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        try
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Неверный email или пароль" });
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Неверный email или пароль" });
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при входе");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}