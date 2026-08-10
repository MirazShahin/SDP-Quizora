using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class PracticeCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }          // optional
    public int Order { get; set; }

    public ICollection<PracticeQuestion> Questions { get; set; } = new List<PracticeQuestion>();
}