using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class InterviewTopic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int Order { get; set; }

    public ICollection<InterviewQA> QAs { get; set; } = new List<InterviewQA>();
}