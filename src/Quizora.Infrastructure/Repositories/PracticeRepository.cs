using Microsoft.EntityFrameworkCore;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Repositories;

public class PracticeRepository : IPracticeRepository
{
    private readonly ApplicationDbContext _context;

    public PracticeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PracticeCategory>> GetAllCategoriesAsync()
    {
        return await _context.PracticeCategories
            .OrderBy(c => c.Order)
            .ToListAsync();
    }

    public async Task<PracticeCategory?> GetCategoryWithQuestionsAsync(Guid categoryId)
    {
        return await _context.PracticeCategories
            .Include(c => c.Questions.OrderBy(q => q.Order))
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task AddAttemptAsync(PracticeAttempt attempt)
    {
        await _context.PracticeAttempts.AddAsync(attempt);
    }

    public async Task<List<PracticeAttempt>> GetAttemptsByUserAsync(Guid userId)
    {
        return await _context.PracticeAttempts
            .Include(a => a.Category)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CompletedAt)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}