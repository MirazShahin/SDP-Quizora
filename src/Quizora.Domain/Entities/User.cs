using Quizora.Domain.Common;
using Quizora.Domain.Enums;

namespace Quizora.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Navigation
    public Company? Company { get; set; }
    public Candidate? Candidate { get; set; }
}