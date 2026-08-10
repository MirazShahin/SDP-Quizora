namespace Quizora.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Type { get; set; } = "Info"; // Result | InterviewCall | Info
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}