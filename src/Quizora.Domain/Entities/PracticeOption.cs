using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class PracticeOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    public PracticeQuestion Question { get; set; } = null!;
}