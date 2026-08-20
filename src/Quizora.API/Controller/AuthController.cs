using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Auth;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Result<AuthResponseDto>>> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return Result<AuthResponseDto>.Failure("Full name is required");

        var emailError = ValidateEmail(dto.Email);
        if (emailError != null)
            return Result<AuthResponseDto>.Failure(emailError);

        var passwordError = ValidatePassword(dto.Password);
        if (passwordError != null)
            return Result<AuthResponseDto>.Failure(passwordError);

        var existingUser = await _userRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (existingUser != null)
            return Result<AuthResponseDto>.Failure("Email already exists");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };

        if (dto.Role == UserRole.Company)
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return Result<AuthResponseDto>.Failure("Company name is required");

            user.Company = new Company
            {
                CompanyName = dto.CompanyName.Trim()
            };
        }
        else if (dto.Role == UserRole.Candidate)
        {
            user.Candidate = new Candidate();
        }
        else
        {
            return Result<AuthResponseDto>.Failure("Invalid role");
        }

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var response = new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = token
        };

        return Result<AuthResponseDto>.Success(response, "Registration successful");
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponseDto>>> Login(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure("Invalid email or password");

        var token = GenerateJwtToken(user);
        var response = new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = token
        };

        return Result<AuthResponseDto>.Success(response, "Login successful");
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string? ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Password is required";

        if (password.Length < 8)
            return "Password must be at least 8 characters";

        if (!password.Any(char.IsUpper))
            return "Password must contain at least one uppercase letter (A-Z)";

        if (!password.Any(char.IsLower))
            return "Password must contain at least one lowercase letter (a-z)";

        if (!password.Any(char.IsDigit))
            return "Password must contain at least one number (0-9)";

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            return "Password must contain at least one special character (!@#$%^&* etc.)";

        return null;
    }

    private static string? ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "Email is required";

        email = email.Trim();

        var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email, pattern))
            return "Please enter a valid email address (e.g. name@gmail.com)";

        var domain = email.ToLowerInvariant().Split('@').LastOrDefault() ?? "";

        var typos = new HashSet<string>
        {
            "gmail.cm", "gmail.con", "gmail.co", "gmail.cpm", "gmail.om", "gmail.comm",
            "gmal.com", "gmial.com", "gamil.com", "gmaill.com",
            "yahoo.cm", "yahoo.con", "yaho.com",
            "outlook.cm", "outlook.con",
            "hotmail.cm", "hotmail.con"
        };

        if (typos.Contains(domain))
            return "Email domain looks wrong. Did you mean gmail.com / yahoo.com / outlook.com?";

        return null;
    }
    [HttpPost("forgot-password")]
    public async Task<ActionResult<Result<ForgotPasswordResponseDto>>> ForgotPassword(ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return Result<ForgotPasswordResponseDto>.Failure("Email is required");

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return Result<ForgotPasswordResponseDto>.Failure("No account found with this email");

        var token = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
        await _userRepository.SaveChangesAsync();

        // Blazor URL — নিজের Web পোর্ট অনুযায়ী বদলাও
        var blazorBase = "https://localhost:7229";
        var resetLink = $"{blazorBase}/reset-password?email={Uri.EscapeDataString(email)}&token={token}";

        var response = new ForgotPasswordResponseDto
        {
            Message = "Reset link generated. Click the link below to set a new password.",
            ResetLink = resetLink
        };

        return Result<ForgotPasswordResponseDto>.Success(response, "Reset link ready");
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<Result>> ResetPassword(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token))
            return Result.Failure("Invalid reset request");

        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return Result.Failure("New password is required");

        if (dto.NewPassword != dto.ConfirmPassword)
            return Result.Failure("Passwords do not match");

        var passwordError = ValidatePassword(dto.NewPassword);
        if (passwordError != null)
            return Result.Failure(passwordError);

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByResetTokenAsync(email, dto.Token);

        if (user == null)
            return Result.Failure("Invalid or expired reset link. Please request a new one.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userRepository.SaveChangesAsync();

        return Result.Success("Password reset successfully. You can now login.");
    }
}