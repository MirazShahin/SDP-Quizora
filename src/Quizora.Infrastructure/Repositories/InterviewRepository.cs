using Microsoft.EntityFrameworkCore;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Repositories;

public class InterviewRepository : IInterviewRepository
{
    private readonly ApplicationDbContext _context;

    public InterviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InterviewTopic>> GetAllTopicsAsync()
    {
        return await _context.InterviewTopics
            .OrderBy(t => t.Order)
            .ToListAsync();
    }

    public async Task<InterviewTopic?> GetTopicWithQAsAsync(Guid topicId)
    {
        return await _context.InterviewTopics
            .Include(t => t.QAs.OrderBy(q => q.Order))
            .FirstOrDefaultAsync(t => t.Id == topicId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}