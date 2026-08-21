using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Quizora.Application.Interfaces;

namespace Quizora.Infrastructure.Services;

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
        var percentColor = percentage >= 70 ? "#16a34a" : percentage >= 50 ? "#d97706" : "#dc2626";

        var body = $@"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;'>
  <div style='max-width:560px;margin:32px auto;background:#ffffff;border-radius:14px;overflow:hidden;'>
    <div style='background:linear-gradient(135deg,#0f172a,#1e3a8a);padding:28px 32px;color:#fff;'>
      <div style='font-size:12px;letter-spacing:2px;opacity:.85;'>QUIZORA</div>
      <h1 style='margin:8px 0 0;font-size:22px;'>Test Result</h1>
    </div>
    <div style='padding:28px 32px;color:#0f172a;'>
      <p>Dear <strong>{Escape(candidateName)}</strong>,</p>
      <p>Your result for <strong>{Escape(testTitle)}</strong> at <strong>{Escape(companyName)}</strong> is ready.</p>
      <div style='background:#f8fafc;border:1px solid #e2e8f0;border-radius:10px;padding:16px;margin:20px 0;text-align:center;'>
        <div style='font-size:13px;color:#64748b;'>Score</div>
        <div style='font-size:32px;font-weight:700;color:{percentColor};'>{score} / {totalQuestions}</div>
        <div style='color:{percentColor};font-weight:600;'>{percentage}%</div>
      </div>
      <p style='color:#64748b;font-size:13px;'>— Team Quizora</p>
    </div>
  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, $"Quizora - Result: {testTitle}", body);
    }

    public async Task SendInterviewCallEmailAsync(
        string toEmail,
        string candidateName,
        string testTitle,
        string companyName,
        int score,
        int totalQuestions,
        double percentage)
    {
        var body = $@"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;'>
  <div style='max-width:560px;margin:32px auto;background:#ffffff;border-radius:14px;overflow:hidden;'>
    <div style='background:linear-gradient(135deg,#0f172a,#7c3aed);padding:28px 32px;color:#fff;'>
      <div style='font-size:12px;letter-spacing:2px;opacity:.85;'>QUIZORA</div>
      <h1 style='margin:8px 0 0;font-size:22px;'>Interview Invitation</h1>
    </div>
    <div style='padding:28px 32px;color:#0f172a;'>
      <p>Dear <strong>{Escape(candidateName)}</strong>,</p>
      <p>
        Congratulations! Based on your performance in <strong>{Escape(testTitle)}</strong>
        ({score}/{totalQuestions}, {percentage}%), <strong>{Escape(companyName)}</strong>
        would like to invite you for an interview.
      </p>
      <p style='color:#64748b;font-size:13px;'>— Team Quizora</p>
    </div>
  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, $"Quizora - Interview Call: {testTitle}", body);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
    {
        var body = $@"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;'>
  <div style='max-width:560px;margin:32px auto;background:#fff;border-radius:14px;overflow:hidden;'>
    <div style='background:linear-gradient(135deg,#0f172a,#1e3a8a);padding:28px 32px;color:#fff;'>
      <div style='font-size:12px;letter-spacing:2px;opacity:.85;'>QUIZORA</div>
      <h1 style='margin:8px 0 0;font-size:22px;'>Reset Password</h1>
    </div>
    <div style='padding:28px 32px;color:#0f172a;'>
      <p>Dear <strong>{Escape(fullName)}</strong>,</p>
      <p>We received a request to reset your password. Click the button below:</p>
      <p style='text-align:center;margin:28px 0;'>
        <a href='{resetLink}'
           style='display:inline-block;background:#4f46e5;color:#fff;text-decoration:none;
                  padding:12px 28px;border-radius:8px;font-weight:600;'>
          Reset Password
        </a>
      </p>
      <p style='font-size:13px;color:#64748b;'>This link expires in <strong>30 minutes</strong>.</p>
      <p style='font-size:13px;color:#64748b;'>If you did not request this, ignore this email.</p>
    </div>
  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, "Quizora - Reset Your Password", body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var from = _config["Email:From"] ?? throw new InvalidOperationException("Email:From missing");
        var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
        var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var username = _config["Email:Username"] ?? from;
        var password = _config["Email:Password"] ?? throw new InvalidOperationException("Email:Password missing");

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(username, password)
        };

        using var message = new MailMessage(from, toEmail, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
    }

    private static string Escape(string? value) =>
        WebUtility.HtmlEncode(value ?? string.Empty);
}