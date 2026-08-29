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
    public string Status { get; set; } = string.Empty;

    public int TabSwitches { get; set; }
    public int FocusLost { get; set; }
    public int PasteAttempts { get; set; }
    public int CopyAttempts { get; set; }

    public bool NeedsReview => TabSwitches >= 5 || PasteAttempts >= 3 || FocusLost >= 8;
}
