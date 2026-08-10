using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class QuestionBankOption : BaseEntity
{
    public Guid QuestionBankId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    public QuestionBank QuestionBank { get; set; } = null!;
}