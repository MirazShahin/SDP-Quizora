namespace Quizora.Application.DTOs.Questions;

public class WeakTopicDto
{
    public string Topic { get; set; } = "";
    public int Score { get; set; }
    public int Total { get; set; }
    public double Accuracy { get; set; }
    public string Severity { get; set; } = "";
    public string Source { get; set; } = "";
}

public class PerformanceSummaryDto
{
    public int PracticeSessions { get; set; }
    public int MockSessions { get; set; }
    public double AvgAccuracy { get; set; }
    public int WeakCount { get; set; }
    public string? StrongestTopic { get; set; }
    public List<WeakTopicDto> WeakTopics { get; set; } = new();
}
