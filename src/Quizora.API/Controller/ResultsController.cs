using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Attempts;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using System.Security.Claims;
using System.IO;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Company")]
public class ResultsController : ControllerBase
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITestRepository _testRepository;
    private readonly INotificationRepository _notificationRepository;

    public ResultsController(
        IInvitationRepository invitationRepository,
        IUserRepository userRepository,
        ITestRepository testRepository,
        INotificationRepository notificationRepository)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _testRepository = testRepository;
        _notificationRepository = notificationRepository;
    }

    [HttpGet("test/{testId}")]
    public async Task<IActionResult> GetResultsByTest(Guid testId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Ok(Result<List<ResultDto>>.Failure("Not authenticated"));

            var userId = Guid.Parse(userIdClaim);
            var user = await _userRepository.GetByIdAsync(userId);

            if (user?.Company == null)
                return Ok(Result<List<ResultDto>>.Failure("Company not found"));

            var test = await _testRepository.GetByIdAsync(testId);
            if (test == null || test.CompanyId != user.Company.Id)
                return Ok(Result<List<ResultDto>>.Failure("Unauthorized or test not found"));

            var invitations = await _invitationRepository.GetByTestIdAsync(testId);

            var results = invitations
                .Where(i => i.Attempt != null)
                .Select(i => new ResultDto
                {
                    InvitationId = i.Id,
                    CandidateName = i.Candidate?.User?.FullName ?? "Unknown",
                    CandidateEmail = i.Candidate?.User?.Email ?? "",
                    Score = i.Attempt!.Score,
                    TotalQuestions = i.Attempt.TotalQuestions,
                    Percentage = i.Attempt.TotalQuestions == 0
                        ? 0
                        : Math.Round((double)i.Attempt.Score / i.Attempt.TotalQuestions * 100, 2),
                    SubmittedAt = i.Attempt.SubmittedAt
                })
                .ToList();

            return Ok(Result<List<ResultDto>>.Success(results));
        }
        catch (Exception ex)
        {
            return Ok(Result<List<ResultDto>>.Failure($"Server error: {ex.Message}"));
        }
    }

    /// <summary>Score notification → Candidate dashboard</summary>
    [HttpPost("send-email/{invitationId}")]
    public async Task<IActionResult> SendResultNotification(Guid invitationId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var companyUser = await _userRepository.GetByIdAsync(userId);
            if (companyUser?.Company == null)
                return Ok(Result.Failure("Company not found"));

            var invitation = await _invitationRepository.GetByIdAsync(invitationId);
            if (invitation?.Attempt == null)
                return Ok(Result.Failure("Result not found"));

            var test = await _testRepository.GetByIdAsync(invitation.TestId);
            if (test == null || test.CompanyId != companyUser.Company.Id)
                return Ok(Result.Failure("Unauthorized"));

            var candidateUserId = invitation.Candidate?.UserId
                                  ?? invitation.Candidate?.User?.Id;
            if (candidateUserId == null || candidateUserId == Guid.Empty)
                return Ok(Result.Failure("Candidate user not found"));

            var score = invitation.Attempt.Score;
            var total = invitation.Attempt.TotalQuestions;
            var pct = total == 0 ? 0 : Math.Round((double)score / total * 100, 2);
            var companyName = companyUser.Company.CompanyName ?? "Company";

            await _notificationRepository.AddAsync(new Notification
            {
                UserId = candidateUserId.Value,
                Title = $"Result: {test.Title}",
                Message = $"Your score for \"{test.Title}\" ({companyName}): {score}/{total} ({pct}%).",
                Type = "Result"
            });
            await _notificationRepository.SaveChangesAsync();

            return Ok(Result.Success("Result notification sent to candidate"));
        }
        catch (Exception ex)
        {
            return Ok(Result.Failure(ex.Message));
        }
    }

    /// <summary>Interview call notification (35/50 or 70%+) → Candidate dashboard</summary>
    [HttpPost("send-interview-call/{invitationId}")]
    public async Task<IActionResult> SendInterviewCallNotification(Guid invitationId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var companyUser = await _userRepository.GetByIdAsync(userId);
            if (companyUser?.Company == null)
                return Ok(Result.Failure("Company not found"));

            var invitation = await _invitationRepository.GetByIdAsync(invitationId);
            if (invitation?.Attempt == null)
                return Ok(Result.Failure("Result not found. Candidate has not submitted yet."));

            var test = await _testRepository.GetByIdAsync(invitation.TestId);
            if (test == null || test.CompanyId != companyUser.Company.Id)
                return Ok(Result.Failure("Unauthorized"));

            var score = invitation.Attempt.Score;
            var total = invitation.Attempt.TotalQuestions;
            var percentage = total == 0 ? 0 : Math.Round((double)score / total * 100, 2);

            bool eligible = total == 50 ? score >= 35 : percentage >= 70;
            if (!eligible)
                return Ok(Result.Failure(
                    $"Not eligible. Need 35/50 or 70%+. Got {score}/{total} ({percentage}%)."));

            var candidateUserId = invitation.Candidate?.UserId
                                  ?? invitation.Candidate?.User?.Id;
            if (candidateUserId == null || candidateUserId == Guid.Empty)
                return Ok(Result.Failure("Candidate user not found"));

            var companyName = companyUser.Company.CompanyName
                              ?? companyUser.FullName
                              ?? "Company";

            await _notificationRepository.AddAsync(new Notification
            {
                UserId = candidateUserId.Value,
                Title = $"Interview Call: {test.Title}",
                Message =
                    $"Congratulations! {companyName} shortlisted you for the next interview round " +
                    $"based on \"{test.Title}\" (Score: {score}/{total}, {percentage}%). " +
                    "They will contact you soon.",
                Type = "InterviewCall"
            });
            await _notificationRepository.SaveChangesAsync();

            return Ok(Result.Success("Interview call notification sent to candidate"));
        }
        catch (Exception ex)
        {
            return Ok(Result.Failure($"Failed to send interview call: {ex.Message}"));
        }
    }
    [HttpGet("test/{testId}/export")]
    public async Task<IActionResult> ExportCsv(Guid testId)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.Company == null)
                return Forbid();

            var test = await _testRepository.GetByIdAsync(testId);
            if (test == null || test.CompanyId != user.Company.Id)
                return NotFound();

            var invitations = await _invitationRepository.GetByTestIdAsync(testId);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Username,Email,Score,TotalQuestions,Percentage,Eligible,SubmittedAt");

            foreach (var i in invitations.Where(x => x.Attempt != null))
            {
                var score = i.Attempt!.Score;
                var total = i.Attempt.TotalQuestions;
                var pct = total == 0 ? 0 : Math.Round((double)score / total * 100, 2);
                var eligible = total == 50 ? score >= 35 : pct >= 70;

                var name = Csv(i.Candidate?.User?.FullName ?? "Unknown");
                var email = Csv(i.Candidate?.User?.Email ?? "");
                var submitted = i.Attempt.SubmittedAt.ToString("yyyy-MM-dd HH:mm");

                sb.AppendLine($"{name},{email},{score},{total},{pct},{(eligible ? "Yes" : "No")},{submitted}");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            var fileName = $"results-{Sanitize(test.Title)}-{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static string Csv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static string Sanitize(string title)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            title = title.Replace(c, '_');
        return string.IsNullOrWhiteSpace(title) ? "test" : title.Trim();
    }
}