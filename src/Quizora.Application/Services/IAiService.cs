namespace Quizora.Application.Interfaces;

public interface IAiService
{
    Task<string> GetCoachFeedbackAsync(string question, string? userAnswer = null);
    Task<string> GetMockInterviewReplyAsync(List<ChatMessageDto> history, string userMessage);
    Task<List<string>> GetWeakTopicsAsync(List<TopicScoreDto> history);
    Task<string> GetAssistantReplyAsync(List<ChatMessageDto> history, string message);
}

public class ChatMessageDto
{
    public string Role { get; set; } = "user"; // system | user | assistant
    public string Content { get; set; } = "";
}

public class TopicScoreDto
{
    public string Topic { get; set; } = "";
    public int Score { get; set; }
    public int Total { get; set; }
}