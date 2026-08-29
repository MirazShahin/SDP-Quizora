using Microsoft.EntityFrameworkCore;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Repositories;

public class AttemptRepository : IAttemptRepository
{
    private readonly ApplicationDbContext _context;

    public AttemptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TestAttempt?> GetByInvitationIdAsync(Guid invitationId)
    {
        return await _context.TestAttempts
            .Include(a => a.Answers)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.InvitationId == invitationId);
    }

    public async Task AddAsync(TestAttempt attempt)
    {
        await _context.TestAttempts.AddAsync(attempt);
        await _context.SaveChangesAsync(); // ← must save
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<TestAttempt>> GetByTestIdAsync(Guid testId)
    {
        return await _context.TestAttempts
            .Include(a => a.Invitation)
                .ThenInclude(i => i.Candidate)
                    .ThenInclude(c => c.User)
            .Include(a => a.Invitation)
                .ThenInclude(i => i.Test)
            .Where(a => a.Invitation.TestId == testId)
            .ToListAsync();
    }
}