using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class Question : BaseEntity
{
    public Guid TestId { get; set; }
    public string Text { get; set; } = string.Empty;

    /// <summary>MCQ | ShortAnswer | Coding</summary>
    public string QuestionType { get; set; } = "MCQ";

    public int Order { get; set; }

    // Coding / ShortAnswer helpers (optional)
    public string? SampleInput { get; set; }
    public string? SampleOutput { get; set; }
    public string? StarterCode { get; set; }

    public Test Test { get; set; } = null!;
    public ICollection<Option> Options { get; set; } = new List<Option>();
}