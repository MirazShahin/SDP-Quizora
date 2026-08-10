namespace Quizora.Application.DTOs.Invitations;

public class InviteCandidateDto
{
    public Guid TestId { get; set; }
    public string CandidateEmail { get; set; } = string.Empty;
}