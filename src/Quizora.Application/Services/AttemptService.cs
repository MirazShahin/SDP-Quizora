using Quizora.Application.Common;
using Quizora.Application.DTOs.Attempts;
using Quizora.Application.DTOs.Questions;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Domain.Enums;

namespace Quizora.Application.Services;

public class AttemptService : IAttemptService
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IAttemptRepository _attemptRepository;
    private readonly ITestRepository _testRepository;
    private readonly IUserRepository _userRepository;
    private readonly IQuestionBankRepository _questionBankRepository;
    public AttemptService(
     IInvitationRepository invitationRepository,
     IAttemptRepository attemptRepository,
     ITestRepository testRepository,
     IUserRepository userRepository,
     IQuestionBankRepository questionBankRepository)
    {
        _invitationRepository = invitationRepository;
        _attemptRepository = attemptRepository;
        _testRepository = testRepository;
        _userRepository = userRepository;
        _questionBankRepository = questionBankRepository;
    }

    public async Task<Result<List<QuestionDto>>> GetQuestionsAsync(Guid userId, Guid invitationId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Candidate == null)
            return Result<List<QuestionDto>>.Failure("Candidate not found");

        var invitation = await _invitationRepository.GetByIdAsync(invitationId);
        if (invitation == null)
            return Result<List<QuestionDto>>.Failure("Invitation not found");

        if (invitation.CandidateId != user.Candidate.Id)
            return Result<List<QuestionDto>>.Failure("Unauthorized");

        if (invitation.Status == InvitationStatus.Completed)
            return Result<List<QuestionDto>>.Failure("Test already completed");

        if (invitation.Status == InvitationStatus.Expired)
            return Result<List<QuestionDto>>.Failure("Invitation expired");

        // আগে অ্যাসাইন করা আছে কিনা চেক
        var existing = await _questionBankRepository.GetInvitationQuestionsAsync(invitationId);

        List<QuestionBank> questions;

        if (existing.Any())
        {
            var ids = existing.Select(e => e.QuestionBankId).ToList();
            questions = await _questionBankRepository.GetByIdsAsync(ids);
            questions = questions.OrderBy(q => ids.IndexOf(q.Id)).ToList();
        }
        else
        {
            // Mix: mostly MCQ + up to 3 coding problems from own bank
            questions = await _questionBankRepository.GetRandomOfficialQuestionsAsync(50);
            if (questions.Count < 10)
                return Result<List<QuestionDto>>.Failure("Not enough questions in the Official bank");

            await _questionBankRepository.SaveInvitationQuestionsAsync(invitationId, questions);
        }

        var dto = questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            QuestionType = string.IsNullOrWhiteSpace(q.QuestionType) ? "MCQ" : q.QuestionType,
            SampleInput = q.SampleInput,
            SampleOutput = q.SampleOutput,
            StarterCode = q.StarterCode ?? q.SampleAnswer,
            Options = q.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text
            }).ToList()
        }).ToList();

        return Result<List<QuestionDto>>.Success(dto);
    }

    public async Task<Result<ResultDto>> SubmitTestAsync(Guid userId, Guid invitationId, SubmitTestDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Candidate == null)
            return Result<ResultDto>.Failure("Candidate not found");

        var invitation = await _invitationRepository.GetByIdAsync(invitationId);
        if (invitation == null)
            return Result<ResultDto>.Failure("Invitation not found");

        if (invitation.CandidateId != user.Candidate.Id)
            return Result<ResultDto>.Failure("Unauthorized");

        if (invitation.Status == InvitationStatus.Completed)
            return Result<ResultDto>.Failure("Test already completed");

        if (invitation.Status == InvitationStatus.Expired)
            return Result<ResultDto>.Failure("Invitation expired");

        var assigned = await _questionBankRepository.GetInvitationQuestionsAsync(invitationId);
        var bankIds = assigned.Select(a => a.QuestionBankId).ToList();
        var bankQuestions = await _questionBankRepository.GetByIdsAsync(bankIds);
        if (bankQuestions.Count == 0)
        {
            // fallback: test-owned questions
            var test = await _testRepository.GetByIdWithQuestionsAsync(invitation.TestId);
            if (test == null)
                return Result<ResultDto>.Failure("Test not found");
            bankQuestions = new List<QuestionBank>();
        }

        var attempt = new TestAttempt
        {
            InvitationId = invitationId,
            SubmittedAt = DateTime.UtcNow,
            TotalQuestions = bankQuestions.Count > 0 ? bankQuestions.Count : dto.Answers.Count
        };

        int correctCount = 0;
        int mcqCount = 0;

        foreach (var answerDto in dto.Answers)
        {
            var question = bankQuestions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
            var type = question?.QuestionType ?? "MCQ";

            if (type.Equals("Coding", StringComparison.OrdinalIgnoreCase)
                || type.Equals("ShortAnswer", StringComparison.OrdinalIgnoreCase))
            {
                attempt.Answers.Add(new Answer
                {
                    QuestionId = answerDto.QuestionId,
                    SelectedOptionId = null,
                    AnswerText = answerDto.AnswerText,
                    IsCorrect = false
                });
                continue;
            }

            mcqCount++;
            var selectedOption = question?.Options.FirstOrDefault(o => o.Id == answerDto.SelectedOptionId);
            bool isCorrect = selectedOption?.IsCorrect ?? false;
            if (isCorrect) correctCount++;

            attempt.Answers.Add(new Answer
            {
                QuestionId = answerDto.QuestionId,
                SelectedOptionId = answerDto.SelectedOptionId,
                AnswerText = answerDto.AnswerText,
                IsCorrect = isCorrect
            });
        }

        attempt.Score = correctCount;
        if (attempt.TotalQuestions == 0)
            attempt.TotalQuestions = Math.Max(mcqCount, dto.Answers.Count);

        await _attemptRepository.AddAsync(attempt);

        invitation.Status = InvitationStatus.Completed;
        invitation.UpdatedAt = DateTime.UtcNow;
        await _invitationRepository.SaveChangesAsync();

        double percentage = attempt.TotalQuestions == 0
            ? 0
            : Math.Round((double)correctCount / attempt.TotalQuestions * 100, 2);

        var resultDto = new ResultDto
        {
            InvitationId = invitationId,
            CandidateName = user.FullName,
            CandidateEmail = user.Email,
            Score = correctCount,
            TotalQuestions = attempt.TotalQuestions,
            Percentage = percentage,
            SubmittedAt = attempt.SubmittedAt,
            Status = "Completed"
        };

        return Result<ResultDto>.Success(resultDto, "Test submitted successfully");

    }
    public async Task<Result<ResultDto>> GetResultAsync(Guid userId, Guid invitationId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<ResultDto>.Failure("User not found");

        var invitation = await _invitationRepository.GetByIdAsync(invitationId);
        if (invitation == null)
            return Result<ResultDto>.Failure("Invitation not found");

        // Candidate নিজের result দেখতে পারবে, Company নিজের test এর result দেখতে পারবে
        bool isCandidate = user.Candidate != null && invitation.CandidateId == user.Candidate.Id;
        bool isCompany = user.Company != null;

        if (!isCandidate && !isCompany)
            return Result<ResultDto>.Failure("Unauthorized");

        if (isCompany)
        {
            var test = await _testRepository.GetByIdAsync(invitation.TestId);
            if (test == null || test.CompanyId != user.Company!.Id)
                return Result<ResultDto>.Failure("Unauthorized");
        }

        var attempt = await _attemptRepository.GetByInvitationIdAsync(invitationId);
        if (attempt == null)
            return Result<ResultDto>.Failure("Result not found");

        var candidateUser = invitation.Candidate?.User;
        double percentage = attempt.TotalQuestions == 0
            ? 0
            : Math.Round((double)attempt.Score / attempt.TotalQuestions * 100, 2);

        var resultDto = new ResultDto
        {
            InvitationId = invitationId,
            CandidateName = candidateUser?.FullName ?? "",
            CandidateEmail = candidateUser?.Email ?? "",
            Score = attempt.Score,
            TotalQuestions = attempt.TotalQuestions,
            Percentage = percentage,
            SubmittedAt = attempt.SubmittedAt,
            Status = invitation.Status.ToString()
        };

        return Result<ResultDto>.Success(resultDto);
    }

    public async Task<Result<List<ResultDto>>> GetResultsByTestAsync(Guid userId, Guid testId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Company == null)
            return Result<List<ResultDto>>.Failure("Company not found");

        var test = await _testRepository.GetByIdAsync(testId);
        if (test == null)
            return Result<List<ResultDto>>.Failure("Test not found");

        if (test.CompanyId != user.Company.Id)
            return Result<List<ResultDto>>.Failure("Unauthorized");

        var invitations = await _invitationRepository.GetByTestIdAsync(testId);
        var resultList = new List<ResultDto>();

        foreach (var invitation in invitations.Where(i => i.Status == InvitationStatus.Completed))
        {
            var attempt = await _attemptRepository.GetByInvitationIdAsync(invitation.Id);
            if (attempt == null) continue;

            var candidateUser = invitation.Candidate?.User;
            double percentage = attempt.TotalQuestions == 0
                ? 0
                : Math.Round((double)attempt.Score / attempt.TotalQuestions * 100, 2);

            resultList.Add(new ResultDto
            {
                InvitationId = invitation.Id,
                CandidateName = candidateUser?.FullName ?? "",
                CandidateEmail = candidateUser?.Email ?? "",
                Score = attempt.Score,
                TotalQuestions = attempt.TotalQuestions,
                Percentage = percentage,
                SubmittedAt = attempt.SubmittedAt,
                Status = invitation.Status.ToString()
            });
        }

        return Result<List<ResultDto>>.Success(resultList);
    }
}