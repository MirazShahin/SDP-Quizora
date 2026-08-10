using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class PracticeQuestion : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Explanation { get; set; }   // উত্তরের ব্যাখ্যা
    public int Order { get; set; }
    public int Difficulty { get; set; } = 1;   // 1 = Easy, 2 = Medium, 3 = Hard

    public PracticeCategory Category { get; set; } = null!;
    public ICollection<PracticeOption> Options { get; set; } = new List<PracticeOption>();
}