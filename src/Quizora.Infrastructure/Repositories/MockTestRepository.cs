using Microsoft.EntityFrameworkCore;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Repositories;

public class MockTestRepository : IMockTestRepository
{
    private readonly ApplicationDbContext _context;

    public MockTestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MockTest>> GetAllActiveAsync()
    {
        return await _context.MockTests
            .Where(m => m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<MockTest?> GetByIdWithQuestionsAsync(Guid id)
    {
        return await _context.MockTests
            .Include(m => m.Questions.OrderBy(q => q.Order))
                .ThenInclude(mq => mq.PracticeQuestion)
                    .ThenInclude(pq => pq.Options)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAttemptAsync(MockAttempt attempt)
    {
        await _context.MockAttempts.AddAsync(attempt);
    }

    public async Task<List<MockAttempt>> GetAttemptsByUserAsync(Guid userId)
    {
        return await _context.MockAttempts
            .Include(a => a.MockTest)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CompletedAt)
            .ToListAsync();
    }

    public async Task<List<MockAttempt>> GetLeaderboardAsync(Guid mockTestId, int top = 20)
    {
        return await _context.MockAttempts
            .Include(a => a.User)
            .Where(a => a.MockTestId == mockTestId)
            .OrderByDescending(a => a.Score)
            .ThenBy(a => a.TimeTakenInSeconds)   // একই স্কোরে কম সময় আগে
            .Take(top)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}