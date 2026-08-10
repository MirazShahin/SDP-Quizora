using Microsoft.EntityFrameworkCore;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly ApplicationDbContext _context;

    public InvitationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TestInvitation?> GetByIdAsync(Guid id)
    {
        return await _context.TestInvitations
            .Include(i => i.Test)
                .ThenInclude(t => t.Company)
            .Include(i => i.Candidate)
                .ThenInclude(c => c.User)
            .Include(i => i.Attempt)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<TestInvitation>> GetByCandidateIdAsync(Guid candidateId)
    {
        return await _context.TestInvitations
            .Include(i => i.Test)
                .ThenInclude(t => t.Company)
            .Where(i => i.CandidateId == candidateId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }
    public async Task<TestInvitation?> GetByTestAndCandidateAsync(Guid testId, Guid candidateId)
    {
        return await _context.TestInvitations
            .FirstOrDefaultAsync(i => i.TestId == testId && i.CandidateId == candidateId);
    }
    public async Task<List<TestInvitation>> GetByTestIdAsync(Guid testId)
    {
        return await _context.TestInvitations
            .Include(i => i.Candidate)
                .ThenInclude(c => c.User)
            .Include(i => i.Attempt)
            .Where(i => i.TestId == testId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid testId, Guid candidateId)
    {
        return await _context.TestInvitations
            .AnyAsync(i => i.TestId == testId && i.CandidateId == candidateId);
    }

    public async Task AddAsync(TestInvitation invitation)
    {
        await _context.TestInvitations.AddAsync(invitation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}