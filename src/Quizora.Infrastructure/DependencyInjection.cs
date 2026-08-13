using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quizora.Application.Interfaces;
using Quizora.Application.Services;
using Quizora.Application.Validators;
using Quizora.Infrastructure.Persistence;
using Quizora.Infrastructure.Repositories;
using Quizora.Infrastructure.Services;

namespace Quizora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITestRepository, TestRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();
        services.AddScoped<IPracticeRepository, PracticeRepository>();
        services.AddScoped<IInterviewRepository, InterviewRepository>();
        services.AddScoped<IMockTestRepository, MockTestRepository>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITestService, TestService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IAttemptService, AttemptService>();
        services.AddScoped<IQuestionBankRepository, QuestionBankRepository>();
        services.AddMemoryCache();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpClient<QuizApiService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(config["QuizApi:BaseUrl"] ?? "https://quizapi.io/api/v1/");
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddHttpClient<IAiService, AiService>();
        services.AddSingleton<ICodeExecutionService, CodeExecutionService>();
        return services;
    }
}