using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class TestCodingProblem : BaseEntity
{
    public Guid TestId { get; set; }
    public Guid CodingProblemId { get; set; }
    public int Order { get; set; }
    public int Points { get; set; } = 100;

    public Test Test { get; set; } = null!;
    public CodingProblem Problem { get; set; } = null!;
}