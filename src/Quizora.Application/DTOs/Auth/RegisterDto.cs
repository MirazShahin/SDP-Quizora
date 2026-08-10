using Quizora.Domain.Enums;

namespace Quizora.Application.DTOs.Auth;

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? CompanyName { get; set; }   // শুধু Company এর জন্য
}