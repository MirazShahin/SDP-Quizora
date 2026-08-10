using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class MockTestQuestion : BaseEntity
{
    public Guid MockTestId { get; set; }
    public Guid PracticeQuestionId { get; set; }   // Practice Question থেকে নিব
    public int Order { get; set; }

    public MockTest MockTest { get; set; } = null!;
    public PracticeQuestion PracticeQuestion { get; set; } = null!;
}