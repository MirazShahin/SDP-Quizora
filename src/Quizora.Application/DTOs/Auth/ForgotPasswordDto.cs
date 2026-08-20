namespace Quizora.Application.DTOs.Auth;

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

// API response — link UI তে দেখানোর জন্য
public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? ResetLink { get; set; }
}