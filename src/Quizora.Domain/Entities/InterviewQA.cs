using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class InterviewQA : BaseEntity
{
    public Guid TopicId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int Order { get; set; }

    public InterviewTopic Topic { get; set; } = null!;
}