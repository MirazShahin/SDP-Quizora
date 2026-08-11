namespace Quizora.Application.DTOs.Questions;

public class CreateQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "MCQ"; // MCQ | ShortAnswer | Coding
    public string? SampleInput { get; set; }
    public string? SampleOutput { get; set; }
    public string? StarterCode { get; set; }
    public List<CreateOptionDto> Options { get; set; } = new();
}

public class CreateOptionDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
