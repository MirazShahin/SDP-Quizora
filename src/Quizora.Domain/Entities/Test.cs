using Quizora.Domain.Common;
using Quizora.Domain.Enums;

namespace Quizora.Domain.Entities;

public class Test : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TestStatus Status { get; set; } = TestStatus.Draft;
    public int? DurationInMinutes { get; set; }
     
    public int? PassingScore { get; set; } 
    public double? PassingPercent { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<TestInvitation> Invitations { get; set; } = new List<TestInvitation>();
}
