
using Quizora.Domain.Common;
using Quizora.Domain.Entities;

public class ContestRegistration : BaseEntity
{
    public Guid ContestId { get; set; }   // Test.Id
    public Guid UserId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public Test Contest { get; set; } = null!;
    public User User { get; set; } = null!;
}