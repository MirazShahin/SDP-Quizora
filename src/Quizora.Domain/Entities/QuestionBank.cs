using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class QuestionBank : BaseEntity
{
    public string Text { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty; // C#, OOP, SQL, Algorithm...

    public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard

    public string UsageType { get; set; } = "Official";
 
    public string QuestionType { get; set; } = "MCQ";

    public string? SampleAnswer { get; set; }

    public string? Keywords { get; set; }

    public ICollection<QuestionBankOption> Options { get; set; } = new List<QuestionBankOption>();
}