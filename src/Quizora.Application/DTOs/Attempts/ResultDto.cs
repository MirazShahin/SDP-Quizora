namespace Quizora.Application.DTOs.Attempts;

public class ResultDto
{
    public Guid InvitationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;   // ← এই লাইনটা অ্যাড করো
}