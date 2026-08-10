using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface ITestRepository
{
    Task<Test?> GetByIdAsync(Guid id);
    Task<Test?> GetByIdWithQuestionsAsync(Guid id);
    Task<List<Test>> GetByCompanyIdAsync(Guid companyId);
    Task AddAsync(Test test);
    Task AddQuestionAsync(Question question);
    Task SaveChangesAsync();
}