namespace Quizora.Application.DTOs.Coding;

public class CodingTestCaseDto
{
    public Guid? Id { get; set; }
    public string Input { get; set; } = "";
    public string ExpectedOutput { get; set; } = "";
    public bool IsSample { get; set; }
    public int Order { get; set; }
}

public class CodingProblemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Statement { get; set; } = "";
    public string Difficulty { get; set; } = "Easy";
    public int TimeLimitMs { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public int TestCaseCount { get; set; }
    public List<CodingTestCaseDto> TestCases { get; set; } = new();
}

public class CreateCodingProblemDto
{
    public string Title { get; set; } = "";
    public string Statement { get; set; } = "";
    public string Difficulty { get; set; } = "Easy";
    public int TimeLimitMs { get; set; } = 3000;
    public List<CodingTestCaseDto> TestCases { get; set; } = new();
}

public class UpdateCodingProblemDto : CreateCodingProblemDto
{
    public bool IsActive { get; set; } = true;
}