namespace Quizora.Application.DTOs.Invitations;

public class BulkInviteDto
{
    public Guid TestId { get; set; }
      
    public string EmailsText { get; set; } = "";
}

public class BulkInviteResultDto
{
    public int SuccessCount { get; set; }
    public int AlreadyInvitedCount { get; set; }
    public int NotFoundCount { get; set; }
    public List<string> Succeeded { get; set; } = new();
    public List<string> AlreadyInvited { get; set; } = new();
    public List<string> NotFound { get; set; } = new();
}