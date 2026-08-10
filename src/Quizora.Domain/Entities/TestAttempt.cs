using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class TestAttempt : BaseEntity
{
    public Guid InvitationId { get; set; }
    public int Score { get; set; }                 // শুধু Company দেখবে
    public int TotalQuestions { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public TestInvitation Invitation { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}