using Microsoft.EntityFrameworkCore;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(Notification n) => await _context.Notifications.AddAsync(n);

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int take = 30)
        => await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(Guid userId)
        => await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAsReadAsync(Guid id, Guid userId)
    {
        var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (n != null) n.IsRead = true;
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var list = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in list) n.IsRead = true;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}