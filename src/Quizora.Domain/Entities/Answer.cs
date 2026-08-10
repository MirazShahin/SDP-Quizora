using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class Answer : BaseEntity
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }

    public Guid? SelectedOptionId { get; set; }

    public string? AnswerText { get; set; }
     
    public bool IsCorrect { get; set; }

    public TestAttempt Attempt { get; set; } = null!;
    public Question? Question { get; set; }
}