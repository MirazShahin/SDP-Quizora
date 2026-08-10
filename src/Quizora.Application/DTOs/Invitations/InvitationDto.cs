using Quizora.Domain.Enums;

namespace Quizora.Application.DTOs.Invitations;

public class InvitationDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}