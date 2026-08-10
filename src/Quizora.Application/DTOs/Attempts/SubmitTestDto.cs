namespace Quizora.Application.DTOs.Attempts;

public class SubmitTestDto
{
    public Guid InvitationId { get; set; }
    public List<SubmitAnswerDto> Answers { get; set; } = new();
}

public class SubmitAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }  // MCQ — nullable
    public string? AnswerText { get; set; }
}