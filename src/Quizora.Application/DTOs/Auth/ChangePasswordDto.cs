// Application/DTOs/Auth/ChangePasswordDto.cs
namespace Quizora.Application.DTOs.Auth;

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public class CandidateCvDto
{
    public Guid CandidateId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}