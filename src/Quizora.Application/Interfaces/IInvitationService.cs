using Quizora.Application.Common;
using Quizora.Application.DTOs.Invitations;

namespace Quizora.Application.Interfaces;

public interface IInvitationService
{
    Task<Result<List<InvitationDto>>> GetMyInvitationsAsync(Guid userId);
    Task<Result> InviteCandidateAsync(Guid userId, InviteCandidateDto dto);
    Task<Result<BulkInviteResultDto>> BulkInviteAsync(Guid userId, BulkInviteDto dto);
}