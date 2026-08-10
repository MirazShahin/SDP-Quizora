using Microsoft.EntityFrameworkCore;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Repositories;

public class TestRepository : ITestRepository
{
    private readonly ApplicationDbContext _context;

    public TestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Test?> GetByIdAsync(Guid id)
    {
        return await _context.Tests
            .Include(t => t.Company)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Test?> GetByIdWithQuestionsAsync(Guid id)
    {
        return await _context.Tests
            .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Test>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Tests
            .Include(t => t.Questions)
            .Include(t => t.Invitations)
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Test test)
    {
        await _context.Tests.AddAsync(test);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task AddQuestionAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();
    }
}