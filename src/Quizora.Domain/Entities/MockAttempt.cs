using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class MockAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MockTestId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public int TimeTakenInSeconds { get; set; }    // কত সময় নিয়েছে
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public MockTest MockTest { get; set; } = null!;
}