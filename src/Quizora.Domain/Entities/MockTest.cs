using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class MockTest : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationInMinutes { get; set; }     // টাইমার
    public int TotalQuestions { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MockTestQuestion> Questions { get; set; } = new List<MockTestQuestion>();
    public ICollection<MockAttempt> Attempts { get; set; } = new List<MockAttempt>();
}