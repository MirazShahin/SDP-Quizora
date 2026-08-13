using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class CodingTestCase : BaseEntity
{
    public Guid CodingProblemId { get; set; }
    public string Input { get; set; } = "";       // stdin
    public string ExpectedOutput { get; set; } = "";
    public bool IsSample { get; set; }             // true = candidate দেখতে পাবে
    public int Order { get; set; }

    public CodingProblem Problem { get; set; } = null!;
}