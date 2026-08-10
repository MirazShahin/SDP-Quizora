using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class Option : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Navigation
    public Question Question { get; set; } = null!;
}