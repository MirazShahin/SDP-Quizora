using Microsoft.EntityFrameworkCore;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;
using System.Text.Json;

namespace Quizora.Infrastructure.Seed;

public static class QuestionBankSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedMcqFromJsonAsync(context);
        await SeedCodingProblemsAsync(context);
    }

    private static async Task SeedMcqFromJsonAsync(ApplicationDbContext context)
    {
        if (await context.QuestionBanks.AnyAsync(q => q.QuestionType == "MCQ" || q.QuestionType == null || q.QuestionType == ""))
            return;

        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "questions.json");
        if (!File.Exists(path))
            path = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "questions.json");
        if (!File.Exists(path))
            return;

        var json = await File.ReadAllTextAsync(path);
        var questions = JsonSerializer.Deserialize<List<QuestionSeedDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (questions == null || !questions.Any()) return;

        foreach (var q in questions)
        {
            context.QuestionBanks.Add(new QuestionBank
            {
                Text = q.Text,
                Category = q.Category ?? string.Empty,
                Difficulty = string.IsNullOrWhiteSpace(q.Difficulty) ? "Medium" : q.Difficulty,
                UsageType = string.IsNullOrWhiteSpace(q.UsageType) ? "Official" : q.UsageType,
                QuestionType = string.IsNullOrWhiteSpace(q.QuestionType) ? "MCQ" : q.QuestionType,
                SampleAnswer = q.SampleAnswer,
                Keywords = q.Keywords,
                SampleInput = q.SampleInput,
                SampleOutput = q.SampleOutput,
                StarterCode = q.StarterCode,
                Options = (q.Options ?? new List<OptionSeedDto>())
                    .Select(o => new QuestionBankOption { Text = o.Text, IsCorrect = o.IsCorrect })
                    .ToList()
            });
        }
        await context.SaveChangesAsync();
    }

    /// <summary>Own coding bank — full program submit (not LeetCode API).</summary>
    public static async Task SeedCodingProblemsAsync(ApplicationDbContext context)
    {
        if (await context.QuestionBanks.AnyAsync(q => q.QuestionType == "Coding"))
            return;

        var coding = new List<QuestionBank>
        {
            new()
            {
                Text = "Two Sum\n\nGiven N integers and a target, print indices (0-based) of two numbers that add up to target.\nAssume exactly one solution. Full program: read input from stdin and print output.",
                Category = "Array",
                Difficulty = "Easy",
                UsageType = "Official",
                QuestionType = "Coding",
                SampleInput = "4\n2 7 11 15\n9",
                SampleOutput = "0 1",
                StarterCode = "using System;\nusing System.Linq;\nclass Program {\n  static void Main() {\n    // read N, array, target — print two indices\n  }\n}"
            },
            new()
            {
                Text = "Reverse a String\n\nRead a single line of text and print it reversed. Full program required.",
                Category = "String",
                Difficulty = "Easy",
                UsageType = "Official",
                QuestionType = "Coding",
                SampleInput = "hello",
                SampleOutput = "olleh",
                StarterCode = "using System;\nclass Program {\n  static void Main() {\n    var s = Console.ReadLine();\n    // print reversed\n  }\n}"
            },
            new()
            {
                Text = "Count Vowels\n\nRead a string and print the number of vowels (a,e,i,o,u case-insensitive).",
                Category = "String",
                Difficulty = "Easy",
                UsageType = "Official",
                QuestionType = "Coding",
                SampleInput = "Quizora",
                SampleOutput = "4",
                StarterCode = "using System;\nclass Program {\n  static void Main() {\n    var s = Console.ReadLine() ?? \"\";\n    // count vowels\n  }\n}"
            },
            new()
            {
                Text = "Factorial\n\nRead an integer N (0 <= N <= 12) and print N!.",
                Category = "Math",
                Difficulty = "Easy",
                UsageType = "Official",
                QuestionType = "Coding",
                SampleInput = "5",
                SampleOutput = "120",
                StarterCode = "using System;\nclass Program {\n  static void Main() {\n    int n = int.Parse(Console.ReadLine()!);\n    // print factorial\n  }\n}"
            },
            new()
            {
                Text = "Find Maximum\n\nFirst line: N. Second line: N integers. Print the maximum value.",
                Category = "Array",
                Difficulty = "Easy",
                UsageType = "Official",
                QuestionType = "Coding",
                SampleInput = "5\n3 1 9 2 7",
                SampleOutput = "9",
                StarterCode = "using System;\nusing System.Linq;\nclass Program {\n  static void Main() {\n    // read N and array, print max\n  }\n}"
            },
            new()
            {
                Text = "Palindrome Check\n\nRead a word. Print YES if it is a palindrome, otherwise NO (case-sensitive).",
                Category = "String",
                Difficulty = "Easy",
                UsageType = "Official",
                QuestionType = "Coding",
                SampleInput = "level",
                SampleOutput = "YES",
                StarterCode = "using System;\nclass Program {\n  static void Main() {\n    var s = Console.ReadLine() ?? \"\";\n    // YES or NO\n  }\n}"
            },
            new()
            {
                Text = "Sum of Array\n\nFirst line: N. Second line: N integers. Print their sum.",
                Category = "Array",
                Difficulty = "Easy",
                UsageType = "Official",
                QuestionType = "Coding",
                SampleInput = "3\n1 2 3",
                SampleOutput = "6",
                StarterCode = "using System;\nusing System.Linq;\nclass Program {\n  static void Main() {\n    // print sum\n  }\n}"
            },
            new()
            {
                Text = "FizzBuzz (single N)\n\nRead N. For i=1..N print FizzBuzz/Fizz/Buzz/i each on its own line (standard FizzBuzz).",
                Category = "Logic",
                Difficulty = "Easy",
                UsageType = "Official",
                QuestionType = "Coding",
                SampleInput = "5",
                SampleOutput = "1\n2\nFizz\n4\nBuzz",
                StarterCode = "using System;\nclass Program {\n  static void Main() {\n    int n = int.Parse(Console.ReadLine()!);\n    for (int i = 1; i <= n; i++) {\n      // print line\n    }\n  }\n}"
            },
        };

        context.QuestionBanks.AddRange(coding);
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
        public string? SampleInput { get; set; }
        public string? SampleOutput { get; set; }
        public string? StarterCode { get; set; }
        public List<OptionSeedDto>? Options { get; set; }
    }

    private class OptionSeedDto
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
