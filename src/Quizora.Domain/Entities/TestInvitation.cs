using Quizora.Domain.Common;
using Quizora.Domain.Enums;

namespace Quizora.Domain.Entities;

public class TestInvitation : BaseEntity
{
    public Guid TestId { get; set; }
    public Guid CandidateId { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Test Test { get; set; } = null!;
    public Candidate Candidate { get; set; } = null!;
    public TestAttempt? Attempt { get; set; }
}