using Quizora.Domain.Common;

namespace Quizora.Domain.Entities;

public class CodingSubmission : BaseEntity
{
    public Guid InvitationId { get; set; }
    public Guid CodingProblemId { get; set; }

    public string Language { get; set; } = "cpp";
    public string SourceCode { get; set; } = "";
    public string Verdict { get; set; } = "";

    public int PassedCount { get; set; }
    public int TotalCount { get; set; }
    public long MaxTimeMs { get; set; }

    public string? CompileOutput { get; set; }
    public string? DetailJson { get; set; }

    public TestInvitation Invitation { get; set; } = null!;
    public CodingProblem Problem { get; set; } = null!;
}