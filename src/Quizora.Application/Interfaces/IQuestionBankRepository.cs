using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface IQuestionBankRepository
{
    Task<List<QuestionBank>> GetRandomOfficialQuestionsAsync(int count);
    Task<List<QuestionBank>> GetByIdsAsync(List<Guid> ids);
    Task<List<InvitationQuestion>> GetInvitationQuestionsAsync(Guid invitationId);
    Task SaveInvitationQuestionsAsync(Guid invitationId, List<QuestionBank> questions);
}