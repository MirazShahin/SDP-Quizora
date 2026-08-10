namespace Quizora.Web.Models.Trivia;

public class TriviaQuizStart
{
    public string QuizId { get; set; } = "";
    public List<TriviaQuizQuestion> Questions { get; set; } = new();
}

public class TriviaQuizQuestion
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Category { get; set; } = "";
    public List<TriviaQuizOption> Options { get; set; } = new();
}

public class TriviaQuizOption
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
}

public class TriviaSubmit
{
    public string QuizId { get; set; } = "";
    public List<TriviaSubmitAnswer> Answers { get; set; } = new();
}

public class TriviaSubmitAnswer
{
    public string QuestionId { get; set; } = "";
    public string SelectedOptionId { get; set; } = "";
}

public class TriviaScore
{
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage { get; set; }
    public List<TriviaReviewItem> Review { get; set; } = new();
}

public class TriviaReviewItem
{
    public string QuestionId { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public string SelectedOptionText { get; set; } = "";
    public string CorrectOptionText { get; set; } = "";
    public bool IsCorrect { get; set; }
}

public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}