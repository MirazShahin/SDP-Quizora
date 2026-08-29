namespace Quizora.Application.DTOs.Attempts;

public class SubmitTestDto
{
    public Guid InvitationId { get; set; }
    public List<SubmitAnswerDto> Answers { get; set; } = new();
    public CheatSummaryDto? CheatSummary { get; set; }
}

public class SubmitAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public string? AnswerText { get; set; }
}
