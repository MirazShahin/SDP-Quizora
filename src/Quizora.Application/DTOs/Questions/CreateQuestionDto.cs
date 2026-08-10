namespace Quizora.Application.DTOs.Questions;

public class CreateQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "MCQ"; // MCQ | ShortAnswer
    public List<CreateOptionDto> Options { get; set; } = new();
}

public class CreateOptionDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}