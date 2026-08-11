using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class QuestionBank : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    public string UsageType { get; set; } = "Official";
    public string QuestionType { get; set; } = "MCQ"; // MCQ | ShortAnswer | Coding
    public string? SampleAnswer { get; set; }
    public string? Keywords { get; set; }
    public string? SampleInput { get; set; }
    public string? SampleOutput { get; set; }
    public string? StarterCode { get; set; }

    public ICollection<QuestionBankOption> Options { get; set; } = new List<QuestionBankOption>();
}
