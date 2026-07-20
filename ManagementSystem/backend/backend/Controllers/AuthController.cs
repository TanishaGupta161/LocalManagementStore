using backend.DTOs.Auth;
using backend.Models;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using backend.Models.Enums;
namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

  public AuthController(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtService jwtService)
{
    _userRepository = userRepository;
    _passwordService = passwordService;
    _jwtService = jwtService;
}

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                Message = "Email already exists."
            });
        }

        // Create User object
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = _passwordService.HashPassword(request.Password),
           Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Save user to MongoDB
        await _userRepository.CreateAsync(user);

        return Ok(new
        {
            Success = true,
            Message = "User Registered Successfully"
        });
    }

   [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Find user
    var user = await _userRepository.GetByEmailAsync(request.Email);

    if (user == null)
    {
        return BadRequest(new
        {
            Success = false,
            Message = "Invalid email or password."
        });
    }

    // Verify password
    var isPasswordValid = _passwordService.VerifyPassword(
        request.Password,
        user.PasswordHash);

    if (!isPasswordValid)
    {
        return BadRequest(new
        {
            Success = false,
            Message = "Invalid email or password."
        });
    }

    // Generate JWT Token
    var token = _jwtService.GenerateToken(
        user.Id!,
        user.Email,
        user.Role.ToString());

    return Ok(new AuthResponse
    {
        Success = true,
        Message = "Login Successful",
        Token = token,
        Role = user.Role.ToString()
    });
}
}