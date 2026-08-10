using Quizora.Domain.Common;
using static System.Net.Mime.MediaTypeNames;

namespace Quizora.Domain.Entities;

public class Company : BaseEntity
{
    public Guid UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Test> Tests { get; set; } = new List<Test>();
}