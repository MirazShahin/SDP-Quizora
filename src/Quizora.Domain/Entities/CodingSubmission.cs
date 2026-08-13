using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class CodingSubmission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CodingProblemId { get; set; }

    public string Language { get; set; } = "cpp";
    public string SourceCode { get; set; } = "";
    public string Verdict { get; set; } = "";

    public int PassedCount { get; set; }
    public int TotalCount { get; set; }
    public long MaxTimeMs { get; set; }

    public string? CompileOutput { get; set; }
    public string? DetailJson { get; set; }

    public User User { get; set; } = null!;
    public CodingProblem Problem { get; set; } = null!;
}