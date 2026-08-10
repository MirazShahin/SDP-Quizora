using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface IInterviewRepository
{
    Task<List<InterviewTopic>> GetAllTopicsAsync();
    Task<InterviewTopic?> GetTopicWithQAsAsync(Guid topicId);
    Task SaveChangesAsync();
}