using Quizora.Domain.Enums;

namespace Quizora.Application.DTOs.Auth;

public class ProfileDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    // Candidate
    public string? Phone { get; set; }
    public int TestsCompleted { get; set; }
    public int TestsPending { get; set; }

    // Company
    public string? CompanyName { get; set; }
    public string? CompanyDescription { get; set; }
    public int TestsCreated { get; set; }
    public int TotalInvited { get; set; }
}

public class UpdateProfileDto
{
    public string FullName { get; set; } = "";
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyDescription { get; set; }
}
