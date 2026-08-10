using Quizora.Application.Common;
using Quizora.Application.DTOs.Invitations;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Domain.Enums;

namespace Quizora.Application.Services;

public class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly ITestRepository _testRepository;
    private readonly IUserRepository _userRepository;

    public InvitationService(
        IInvitationRepository invitationRepository,
        ITestRepository testRepository,
        IUserRepository userRepository)
    {
        _invitationRepository = invitationRepository;
        _testRepository = testRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<InvitationDto>>> GetMyInvitationsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Candidate == null)
            return Result<List<InvitationDto>>.Failure("Candidate not found");

        var invitations = await _invitationRepository.GetByCandidateIdAsync(user.Candidate.Id);

        var result = invitations.Select(i => new InvitationDto
        {
            Id = i.Id,
            TestId = i.TestId,
            TestTitle = i.Test?.Title ?? "",
            CompanyName = i.Test?.Company?.CompanyName ?? "",
            Status = i.Status,
            CreatedAt = i.CreatedAt,
            CompletedAt = i.CompletedAt
        }).ToList();

        return Result<List<InvitationDto>>.Success(result);
    }

    public async Task<Result> InviteCandidateAsync(Guid userId, InviteCandidateDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Company == null)
            return Result.Failure("Company not found");

        var test = await _testRepository.GetByIdAsync(dto.TestId);
        if (test == null)
            return Result.Failure("Test not found");

        if (test.CompanyId != user.Company.Id)
            return Result.Failure("Unauthorized");

        if (string.IsNullOrWhiteSpace(dto.CandidateEmail))
            return Result.Failure("Email is required");

        var email = dto.CandidateEmail.Trim().ToLowerInvariant();
        var candidateUser = await _userRepository.GetByEmailAsync(email);
        if (candidateUser?.Candidate == null)
            return Result.Failure("Candidate not found with this email");

        var existing = await _invitationRepository.GetByTestAndCandidateAsync(dto.TestId, candidateUser.Candidate.Id);
        if (existing != null)
            return Result.Failure("Candidate already invited to this test");

        var invitation = new TestInvitation
        {
            TestId = dto.TestId,
            CandidateId = candidateUser.Candidate.Id,
            Status = InvitationStatus.Pending
        };

        await _invitationRepository.AddAsync(invitation);
        await _invitationRepository.SaveChangesAsync();

        return Result.Success("Invitation sent successfully");
    }
    public async Task<Result<BulkInviteResultDto>> BulkInviteAsync(Guid userId, BulkInviteDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Company == null)
            return Result<BulkInviteResultDto>.Failure("Company not found");

        var test = await _testRepository.GetByIdAsync(dto.TestId);
        if (test == null)
            return Result<BulkInviteResultDto>.Failure("Test not found");

        if (test.CompanyId != user.Company.Id)
            return Result<BulkInviteResultDto>.Failure("Unauthorized");

        var emails = ParseEmails(dto.EmailsText);
        if (emails.Count == 0)
            return Result<BulkInviteResultDto>.Failure("No valid emails found");

        if (emails.Count > 100)
            return Result<BulkInviteResultDto>.Failure("Max 100 emails per batch");

        var result = new BulkInviteResultDto();

        foreach (var email in emails)
        {
            var candidateUser = await _userRepository.GetByEmailAsync(email);
            if (candidateUser?.Candidate == null)
            {
                result.NotFound.Add(email);
                result.NotFoundCount++;
                continue;
            }

            var existing = await _invitationRepository.GetByTestAndCandidateAsync(
                dto.TestId, candidateUser.Candidate.Id);

            if (existing != null)
            {
                result.AlreadyInvited.Add(email);
                result.AlreadyInvitedCount++;
                continue;
            }

            await _invitationRepository.AddAsync(new TestInvitation
            {
                TestId = dto.TestId,
                CandidateId = candidateUser.Candidate.Id,
                Status = InvitationStatus.Pending
            });

            result.Succeeded.Add(email);
            result.SuccessCount++;
        }

        if (result.SuccessCount > 0)
            await _invitationRepository.SaveChangesAsync();

        return Result<BulkInviteResultDto>.Success(result,
            $"Invited {result.SuccessCount}, already {result.AlreadyInvitedCount}, not found {result.NotFoundCount}");
    }

    private static List<string> ParseEmails(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new();

        return text
            .Split(new[] { ',', ';', '\n', '\r', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant())
            .Where(e => e.Contains('@') && e.Contains('.'))
            .Distinct()
            .ToList();
    }
}