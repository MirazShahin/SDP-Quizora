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
<body style='margin:0; padding:0; background:#f1f5f9; font-family:Segoe UI, Arial, sans-serif;'>
  <div style='max-width:560px; margin:32px auto; background:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 8px 24px rgba(15,23,42,.08);'>
    <div style='background:linear-gradient(135deg,#0f172a,#1e3a8a 50%,#7c3aed); padding:28px 32px; color:#fff;'>
      <div style='font-size:12px; letter-spacing:2px; opacity:.85;'>QUIZORA</div>
      <h1 style='margin:8px 0 0; font-size:22px;'>Test Result</h1>
    </div>
    <div style='padding:28px 32px; color:#0f172a;'>
      <p>Dear <strong>{Escape(candidateName)}</strong>,</p>
      <p>
        Your result for <strong>{Escape(testTitle)}</strong>
        at <strong>{Escape(companyName)}</strong> is ready.
      </p>
      <div style='background:#f8fafc; border:1px solid #e2e8f0; border-radius:10px; padding:16px; margin:20px 0; text-align:center;'>
        <div style='font-size:13px; color:#64748b;'>Score</div>
        <div style='font-size:32px; font-weight:700; color:{percentColor};'>{score} / {totalQuestions}</div>
        <div style='color:{percentColor}; font-weight:600;'>{percentage}%</div>
      </div>
      <p style='color:#64748b; font-size:14px;'>
        Keep practicing and good luck with your next steps.
      </p>
      <p style='margin-top:24px; color:#94a3b8; font-size:13px;'>
        — {Escape(companyName)}<br/>Sent via Quizora
      </p>
    </div>
  </div>
</body>
</html>";

        await SendAsync(
            toEmail,
            $"Your result for {testTitle} – {companyName}",
            body);
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
<body style='margin:0; padding:0; background:#f1f5f9; font-family:Segoe UI, Arial, sans-serif;'>
  <div style='max-width:560px; margin:32px auto; background:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 8px 24px rgba(15,23,42,.08);'>
    <div style='background:linear-gradient(135deg,#0f172a,#16a34a); padding:28px 32px; color:#fff;'>
      <div style='font-size:12px; letter-spacing:2px; opacity:.85;'>QUIZORA</div>
      <h1 style='margin:8px 0 0; font-size:22px;'>Interview Invitation</h1>
    </div>
    <div style='padding:28px 32px; color:#0f172a;'>
      <p>Dear <strong>{Escape(candidateName)}</strong>,</p>
      <p>
        Congratulations! Based on your performance in
        <strong>{Escape(testTitle)}</strong> for
        <strong>{Escape(companyName)}</strong>, you have been
        <strong>shortlisted for the next interview round</strong>.
      </p>
      <div style='background:#f0fdf4; border:1px solid #bbf7d0; border-radius:10px; padding:16px; margin:20px 0; text-align:center;'>
        <div style='font-size:13px; color:#166534;'>Your Score</div>
        <div style='font-size:32px; font-weight:700; color:#15803d;'>{score} / {totalQuestions}</div>
        <div style='color:#166534; font-weight:600;'>{percentage}%</div>
      </div>
      <p>
        Our team will contact you shortly with the interview schedule.
        Please keep an eye on this inbox.
      </p>
      <p style='margin-top:24px; color:#94a3b8; font-size:13px;'>
        — {Escape(companyName)}<br/>Sent via Quizora
      </p>
    </div>
  </div>
</body>
</html>";

        await SendAsync(
            toEmail,
            $"Interview Call – {testTitle} | {companyName}",
            body);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var from = _config["Email:From"] ?? "noreply@quizora.local";
        var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
        var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var user = _config["Email:Username"];
        var pass = _config["Email:Password"];

        using var message = new MailMessage
        {
            From = new MailAddress(from, "Quizora"),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true
        };

        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
            client.Credentials = new NetworkCredential(user, pass);

        await client.SendMailAsync(message);
    }

    private static string Escape(string? value)
        => System.Net.WebUtility.HtmlEncode(value ?? "");
}