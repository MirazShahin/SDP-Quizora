namespace Quizora.Application.DTOs.Questions;

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "MCQ";
    public int Order { get; set; }
    public string? SampleInput { get; set; }
    public string? SampleOutput { get; set; }
    public string? StarterCode { get; set; }
    public List<OptionDto> Options { get; set; } = new();
}

public class OptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
