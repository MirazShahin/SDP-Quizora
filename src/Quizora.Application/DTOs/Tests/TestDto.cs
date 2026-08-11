using Quizora.Domain.Enums;

namespace Quizora.Application.DTOs.Tests;

public class TestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TestStatus Status { get; set; }
    public int? DurationInMinutes { get; set; }
    public int? PassingScore { get; set; }
    public double? PassingPercent { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalInvitations { get; set; }
    public int CompletedCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
