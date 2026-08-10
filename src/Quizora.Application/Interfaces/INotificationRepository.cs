using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<List<Notification>> GetByUserIdAsync(Guid userId, int take = 30);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllReadAsync(Guid userId);
    Task SaveChangesAsync();
}