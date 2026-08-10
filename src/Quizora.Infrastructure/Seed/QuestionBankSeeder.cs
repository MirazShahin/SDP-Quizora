using Microsoft.EntityFrameworkCore;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;
using System.Text.Json;

namespace Quizora.Infrastructure.Seed;

public static class QuestionBankSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.QuestionBanks.AnyAsync())
            return; // already seeded

        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "questions.json");

        if (!File.Exists(path))
        {
            // fallback for development
            path = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "questions.json");
        }

        if (!File.Exists(path))
            return;

        var json = await File.ReadAllTextAsync(path);
        var questions = JsonSerializer.Deserialize<List<QuestionSeedDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (questions == null || !questions.Any())
            return;

        foreach (var q in questions)
        {
            var entity = new QuestionBank
            {
                Text = q.Text,
                Category = q.Category ?? string.Empty,
                Difficulty = string.IsNullOrWhiteSpace(q.Difficulty) ? "Medium" : q.Difficulty,
                UsageType = string.IsNullOrWhiteSpace(q.UsageType) ? "Official" : q.UsageType,
                QuestionType = string.IsNullOrWhiteSpace(q.QuestionType) ? "MCQ" : q.QuestionType,
                SampleAnswer = q.SampleAnswer,
                Keywords = q.Keywords,
                Options = (q.Options ?? new List<OptionSeedDto>())
                    .Select(o => new QuestionBankOption
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    })
                    .ToList()
            };

            context.QuestionBanks.Add(entity);
        }

        await context.SaveChangesAsync();
    }

    private class QuestionSeedDto
    {
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public string UsageType { get; set; } = "Official";
        public string QuestionType { get; set; } = "MCQ";
        public string? SampleAnswer { get; set; }
        public string? Keywords { get; set; }
        public List<OptionSeedDto>? Options { get; set; }
    }

    private class OptionSeedDto
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

}