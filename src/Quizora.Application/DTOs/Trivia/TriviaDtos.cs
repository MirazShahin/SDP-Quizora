using System.Text.Json.Serialization;

namespace Quizora.Application.DTOs.Trivia;

public class TriviaApiResponse
{
    [JsonPropertyName("response_code")]
    public int ResponseCode { get; set; }

    [JsonPropertyName("results")]
    public List<TriviaQuestion> Results { get; set; } = new();
}

public class TriviaQuestion
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "";

    [JsonPropertyName("question")]
    public string Question { get; set; } = "";

    [JsonPropertyName("correct_answer")]
    public string CorrectAnswer { get; set; } = "";

    [JsonPropertyName("incorrect_answers")]
    public List<string> IncorrectAnswers { get; set; } = new();
}

public class PracticeQuestionFromApi
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Category { get; set; } = "";
    public List<PracticeOptionFromApi> Options { get; set; } = new();
}

public class PracticeOptionFromApi
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}

public class TriviaQuizStartDto
{
    public string QuizId { get; set; } = "";
    public List<TriviaQuizQuestionDto> Questions { get; set; } = new();
}

public class TriviaQuizQuestionDto
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Category { get; set; } = "";
    public List<TriviaQuizOptionDto> Options { get; set; } = new();
}

public class TriviaQuizOptionDto
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
}

public class TriviaSubmitDto
{
    public string QuizId { get; set; } = "";
    public List<TriviaSubmitAnswerDto> Answers { get; set; } = new();
}

public class TriviaSubmitAnswerDto
{
    public string QuestionId { get; set; } = "";
    public string SelectedOptionId { get; set; } = "";
}

public class TriviaScoreDto
{
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage { get; set; }
    public List<TriviaReviewItemDto> Review { get; set; } = new();
}

public class TriviaReviewItemDto
{
    public string QuestionId { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public string SelectedOptionText { get; set; } = "";
    public string CorrectOptionText { get; set; } = "";
    public bool IsCorrect { get; set; }
}