using System.Text.Json.Serialization;

namespace Quizora.Application.DTOs.Trivia;

// ===== New QuizAPI envelope =====
public class QuizApiEnvelope
{
    public bool Success { get; set; }
    public List<QuizApiNewQuestion> Data { get; set; } = new();
}

public class QuizApiNewQuestion
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Type { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string? Explanation { get; set; }
    public string Category { get; set; } = "";
    public List<QuizApiNewAnswer> Answers { get; set; } = new();
}

public class QuizApiNewAnswer
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}

// ===== Classic format (fallback) =====
public class QuizApiQuestion
{
    public int Id { get; set; }
    public string Question { get; set; } = "";
    public QuizApiAnswers Answers { get; set; } = new();
    public QuizApiCorrectAnswers CorrectAnswers { get; set; } = new();
    public string Category { get; set; } = "";
    public string Difficulty { get; set; } = "";
}

public class QuizApiAnswers
{
    [JsonPropertyName("answer_a")] public string? AnswerA { get; set; }
    [JsonPropertyName("answer_b")] public string? AnswerB { get; set; }
    [JsonPropertyName("answer_c")] public string? AnswerC { get; set; }
    [JsonPropertyName("answer_d")] public string? AnswerD { get; set; }
    [JsonPropertyName("answer_e")] public string? AnswerE { get; set; }
    [JsonPropertyName("answer_f")] public string? AnswerF { get; set; }
}

public class QuizApiCorrectAnswers
{
    [JsonPropertyName("answer_a_correct")] public string AnswerACorrect { get; set; } = "false";
    [JsonPropertyName("answer_b_correct")] public string AnswerBCorrect { get; set; } = "false";
    [JsonPropertyName("answer_c_correct")] public string AnswerCCorrect { get; set; } = "false";
    [JsonPropertyName("answer_d_correct")] public string AnswerDCorrect { get; set; } = "false";
    [JsonPropertyName("answer_e_correct")] public string AnswerECorrect { get; set; } = "false";
    [JsonPropertyName("answer_f_correct")] public string AnswerFCorrect { get; set; } = "false";
}