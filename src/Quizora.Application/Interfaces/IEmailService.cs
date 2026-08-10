namespace Quizora.Application.Interfaces;

public interface IEmailService
{
    Task SendResultEmailAsync(
        string toEmail,
        string candidateName,
        string testTitle,
        string companyName,
        int score,
        int totalQuestions,
        double percentage);

    Task SendInterviewCallEmailAsync(
        string toEmail,
        string candidateName,
        string testTitle,
        string companyName,
        int score,
        int totalQuestions,
        double percentage);
}