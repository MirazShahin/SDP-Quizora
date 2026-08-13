using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class CodingProblem : BaseEntity
{
    public string Title { get; set; } = "";
    public string Statement { get; set; } = "";
    public string Difficulty { get; set; } = "Easy"; // Easy | Medium | Hard
    public int TimeLimitMs { get; set; } = 3000;
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }

    public ICollection<CodingTestCase> TestCases { get; set; } = new List<CodingTestCase>();
}