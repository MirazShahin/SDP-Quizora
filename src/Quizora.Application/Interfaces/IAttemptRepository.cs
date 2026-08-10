using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface IAttemptRepository
{
    Task<TestAttempt?> GetByInvitationIdAsync(Guid invitationId);
    Task AddAsync(TestAttempt attempt);
    Task SaveChangesAsync();
    Task<List<TestAttempt>> GetByTestIdAsync(Guid testId);
}