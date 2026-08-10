using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface IMockTestRepository
{
    Task<List<MockTest>> GetAllActiveAsync();
    Task<MockTest?> GetByIdWithQuestionsAsync(Guid id);
    Task AddAttemptAsync(MockAttempt attempt);
    Task<List<MockAttempt>> GetAttemptsByUserAsync(Guid userId);
    Task<List<MockAttempt>> GetLeaderboardAsync(Guid mockTestId, int top = 20);
    Task SaveChangesAsync();
}