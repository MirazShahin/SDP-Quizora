using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class PracticeAttempt : BaseEntity
{
    public Guid UserId { get; set; }           // কে প্র্যাকটিস দিয়েছে
    public Guid CategoryId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public PracticeCategory Category { get; set; } = null!;
}