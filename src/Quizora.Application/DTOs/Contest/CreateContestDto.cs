namespace Quizora.Application.DTOs.Contest;

public class CreateContestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationInMinutes { get; set; } = 120;
    public DateTime? ContestStartAt { get; set; }
    public DateTime? ContestEndAt { get; set; }
    /// <summary>List of CodingProblem IDs to include (order matters)</summary>
    public List<Guid> CodingProblemIds { get; set; } = new();
    /// <summary>Optional points per problem (same order as CodingProblemIds). Default 100.</summary>
    public List<int>? Points { get; set; }
}

public class ContestListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int? DurationInMinutes { get; set; }
    public DateTime? ContestStartAt { get; set; }
    public DateTime? ContestEndAt { get; set; }
    public string Status { get; set; } = ""; // Upcoming | Running | Ended | Draft
    public int ProblemCount { get; set; }
    public string CompanyName { get; set; } = "";
}

public class ContestDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int? DurationInMinutes { get; set; }
    public DateTime? ContestStartAt { get; set; }
    public DateTime? ContestEndAt { get; set; }
    public string Status { get; set; } = "";
    public bool IsPublic { get; set; }
    public List<ContestProblemItemDto> Problems { get; set; } = new();
}

public class ContestProblemItemDto
{
    public Guid CodingProblemId { get; set; }
    public string Letter { get; set; } = "A";
    public string Title { get; set; } = "";
    public string Difficulty { get; set; } = "Easy";
    public int Points { get; set; }
    public int Order { get; set; }
    public int TimeLimitMs { get; set; }
}
