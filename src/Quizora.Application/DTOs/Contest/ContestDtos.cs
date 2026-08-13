namespace Quizora.Application.DTOs.Contest;

public class AssignCodingProblemsDto
{
    public List<Guid> CodingProblemIds { get; set; } = new();
}

public class TestCodingProblemDto
{
    public Guid Id { get; set; }
    public Guid CodingProblemId { get; set; }
    public string Letter { get; set; } = "A";
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public int Points { get; set; }
    public int TimeLimitMs { get; set; }
    public int TestCaseCount { get; set; }
}

public class ContestOverviewDto
{
    public Guid InvitationId { get; set; }
    public Guid TestId { get; set; }
    public string TestTitle { get; set; } = "";
    public int? DurationInMinutes { get; set; }
    public List<ContestProblemListItemDto> Problems { get; set; } = new();
}

public class ContestProblemListItemDto
{
    public Guid CodingProblemId { get; set; }
    public string Letter { get; set; } = "A";
    public string Title { get; set; } = "";
    public int Points { get; set; }
    public int TimeLimitMs { get; set; }
    public string Status { get; set; } = "—";
    public int SubmissionCount { get; set; }
}

public class ContestProblemDetailDto
{
    public Guid CodingProblemId { get; set; }
    public string Letter { get; set; } = "A";
    public string Title { get; set; } = "";
    public string Statement { get; set; } = "";
    public int TimeLimitMs { get; set; }
    public int Points { get; set; }
    public List<SampleTestDto> Samples { get; set; } = new();
}

public class SampleTestDto
{
    public string Input { get; set; } = "";
    public string ExpectedOutput { get; set; } = "";
}

public class CodingSubmitRequestDto
{
    public string Language { get; set; } = "cpp";
    public string SourceCode { get; set; } = "";
}

public class CodingSubmitResultDto
{
    public Guid SubmissionId { get; set; }
    public string Verdict { get; set; } = "";
    public int PassedCount { get; set; }
    public int TotalCount { get; set; }
    public long MaxTimeMs { get; set; }
    public string? CompileOutput { get; set; }
    public List<TestCaseResultDto> Cases { get; set; } = new();
}

public class TestCaseResultDto
{
    public int Index { get; set; }
    public bool IsSample { get; set; }
    public bool Passed { get; set; }
    public string Status { get; set; } = "";
    public long TimeMs { get; set; }
    public string? Stdout { get; set; }
    public string? Expected { get; set; }
}