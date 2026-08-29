using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface IInvitationRepository
{
    Task<TestInvitation?> GetByIdAsync(Guid id);
    Task MarkCompletedAsync(Guid invitationId);
    Task<List<TestInvitation>> GetByCandidateIdAsync(Guid candidateId);
    Task<TestInvitation?> GetByTestAndCandidateAsync(Guid testId, Guid candidateId);
    Task<List<TestInvitation>> GetByTestIdAsync(Guid testId);
    Task<bool> ExistsAsync(Guid testId, Guid candidateId);
    Task AddAsync(TestInvitation invitation);
    Task SaveChangesAsync();
}