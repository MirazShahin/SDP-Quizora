using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class TestAttempt : BaseEntity
{
    public Guid InvitationId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Anti-cheat summary (browser events during the test)
    public int TabSwitches { get; set; }
    public int FocusLost { get; set; }
    public int PasteAttempts { get; set; }
    public int CopyAttempts { get; set; }

    public TestInvitation Invitation { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
