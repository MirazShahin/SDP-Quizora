using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class InvitationQuestion : BaseEntity
{
    public Guid InvitationId { get; set; }
    public Guid QuestionBankId { get; set; }
    public int Order { get; set; }   // প্রশ্নের সিরিয়াল

    public TestInvitation Invitation { get; set; } = null!;
    public QuestionBank QuestionBank { get; set; } = null!;
}