using System.Net;
using System.Net.Mail;

namespace Quizora.Infrastructure.Services;

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
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendResultEmailAsync(
        string toEmail,
        string candidateName,
        string testTitle,
        string companyName,
        int score,
        int totalQuestions,
        double percentage)
    {
        var from = _config["Email:From"] ?? "noreply@quizora.com";
        var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
        var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var user = _config["Email:Username"];
        var pass = _config["Email:Password"];

        var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Segoe UI, Arial, sans-serif; background:#f4f6fb; padding:24px;'>
  <div style='max-width:560px; margin:auto; background:#fff; border-radius:12px; overflow:hidden; box-shadow:0 4px 20px rgba(0,0,0,.06);'>
    <div style='background:linear-gradient(135deg,#1e3a8a,#7c3aed); color:#fff; padding:24px 28px;'>
      <div style='font-size:13px; opacity:.85; letter-spacing:1px;'>QUIZORA</div>
      <h2 style='margin:8px 0 0; font-size:22px;'>Assessment Result</h2>
    </div>
    <div style='padding:28px;'>
      <p style='margin:0 0 12px; color:#334155;'>Dear <strong>{candidateName}</strong>,</p>
      <p style='margin:0 0 20px; color:#475569; line-height:1.6;'>
        Thank you for completing the assessment <strong>{testTitle}</strong>
        conducted by <strong>{companyName}</strong>.
      </p>

      <div style='background:#f8fafc; border:1px solid #e2e8f0; border-radius:10px; padding:18px; margin-bottom:20px;'>
        <div style='display:flex; justify-content:space-between; margin-bottom:10px;'>
          <span style='color:#64748b;'>Score</span>
          <strong style='color:#0f172a;'>{score} / {totalQuestions}</strong>
        </div>
        <div style='display:flex; justify-content:space-between;'>
          <span style='color:#64748b;'>Percentage</span>
          <strong style='color:{(percentage >= 60 ? "#16a34a" : "#dc2626")};'>{percentage:0.##}%</strong>
        </div>
      </div>

      <p style='margin:0 0 8px; color:#475569; line-height:1.6;'>
        Our team will review your performance and contact you regarding the next steps if applicable.
      </p>
      <p style='margin:0; color:#475569;'>Best regards,<br/><strong>{companyName}</strong><br/><span style='color:#94a3b8; font-size:13px;'>via Quizora</span></p>
    </div>
  </div>
</body>
</html>";

        using var message = new MailMessage(from, toEmail)
        {
            Subject = $"Your result for {testTitle} – {companyName}",
            Body = body,
            IsBodyHtml = true
        };

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrEmpty(user) ? null : new NetworkCredential(user, pass)
        };

        await client.SendMailAsync(message);
    }
}