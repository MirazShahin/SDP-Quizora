using Quizora.Domain.Common;
using Quizora.Domain.Entities;

public class Candidate : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Phone { get; set; }

    // CV
    public string? CvOriginalName { get; set; }
    public string? CvStoredName { get; set; }
    public long? CvFileSize { get; set; }
    public DateTime? CvUploadedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<TestInvitation> Invitations { get; set; } = new List<TestInvitation>();
}