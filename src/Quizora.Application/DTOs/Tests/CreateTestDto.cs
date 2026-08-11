namespace Quizora.Application.DTOs.Tests;

public class CreateTestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationInMinutes { get; set; }
    public int? PassingScore { get; set; }
    public double? PassingPercent { get; set; }
}
