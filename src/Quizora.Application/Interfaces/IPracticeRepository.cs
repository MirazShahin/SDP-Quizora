using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface IPracticeRepository
{
    Task<List<PracticeCategory>> GetAllCategoriesAsync();
    Task<PracticeCategory?> GetCategoryWithQuestionsAsync(Guid categoryId);
    Task AddAttemptAsync(PracticeAttempt attempt);
    Task<List<PracticeAttempt>> GetAttemptsByUserAsync(Guid userId);
    Task SaveChangesAsync();
}