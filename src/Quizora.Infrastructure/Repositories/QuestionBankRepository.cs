using Microsoft.EntityFrameworkCore;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Repositories;

public class QuestionBankRepository : IQuestionBankRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionBankRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestionBank>> GetRandomOfficialQuestionsAsync(int count)
    {
        // Up to 3 coding problems from own bank + rest MCQ
        var coding = await _context.QuestionBanks
            .Include(q => q.Options)
            .Where(q => q.UsageType == "Official" && q.QuestionType == "Coding")
            .OrderBy(q => Guid.NewGuid())
            .Take(3)
            .ToListAsync();

        var mcqTake = Math.Max(count - coding.Count, 10);
        var mcq = await _context.QuestionBanks
            .Include(q => q.Options)
            .Where(q => q.UsageType == "Official" &&
                        (q.QuestionType == "MCQ" || string.IsNullOrEmpty(q.QuestionType)))
            .OrderBy(q => Guid.NewGuid())
            .Take(mcqTake)
            .ToListAsync();

        return coding.Concat(mcq).OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
    }

    public async Task<List<QuestionBank>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.QuestionBanks
            .Include(q => q.Options)
            .Where(q => ids.Contains(q.Id))
            .ToListAsync();
    }

    public async Task<List<InvitationQuestion>> GetInvitationQuestionsAsync(Guid invitationId)
    {
        return await _context.InvitationQuestions
            .Where(iq => iq.InvitationId == invitationId)
            .OrderBy(iq => iq.Order)
            .ToListAsync();
    }

    public async Task SaveInvitationQuestionsAsync(Guid invitationId, List<QuestionBank> questions)
    {
        var invitationQuestions = questions.Select((q, index) => new InvitationQuestion
        {
            InvitationId = invitationId,
            QuestionBankId = q.Id,
            Order = index + 1
        }).ToList();

        _context.InvitationQuestions.AddRange(invitationQuestions);
        await _context.SaveChangesAsync();
    }
}
