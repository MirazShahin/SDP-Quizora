using Quizora.Domain.Entities;

namespace Quizora.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByResetTokenAsync(string email, string token);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}