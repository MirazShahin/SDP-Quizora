using Microsoft.EntityFrameworkCore;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.Infrastructure.Seed;

public static class CodingProblemSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.CodingProblems.AnyAsync())
            return;

        var problems = new List<CodingProblem>
        {
            new()
            {
                Title = "A. Sum",
                Difficulty = "Easy",
                TimeLimitMs = 1000,
                IsActive = true,
                Order = 1,
                Statement =
@"Time limit: 1 second

You are given two integers a and b.
Print a + b.

Input
Two integers a and b (−1000 ≤ a, b ≤ 1000).

Output
Print a single integer — the sum.

Examples
Input
2 3
Output
5",
                TestCases = new List<CodingTestCase>
                {
                    new() { Input = "2 3", ExpectedOutput = "5", IsSample = true, Order = 1 },
                    new() { Input = "0 0", ExpectedOutput = "0", IsSample = false, Order = 2 },
                    new() { Input = "-5 10", ExpectedOutput = "5", IsSample = false, Order = 3 },
                    new() { Input = "1000 -1000", ExpectedOutput = "0", IsSample = false, Order = 4 },
                }
            },
            new()
            {
                Title = "B. Max of Three",
                Difficulty = "Easy",
                TimeLimitMs = 1000,
                IsActive = true,
                Order = 2,
                Statement =
@"Time limit: 1 second

You are given three integers a, b and c.
Print the maximum of them.

Input
Three integers a, b, c (−10^9 ≤ a, b, c ≤ 10^9).

Output
Print a single integer — the maximum value.

Examples
Input
1 5 3
Output
5

Input
-2 -8 -1
Output
-1",
                TestCases = new List<CodingTestCase>
                {
                    new() { Input = "1 5 3", ExpectedOutput = "5", IsSample = true, Order = 1 },
                    new() { Input = "-2 -8 -1", ExpectedOutput = "-1", IsSample = true, Order = 2 },
                    new() { Input = "100 100 100", ExpectedOutput = "100", IsSample = false, Order = 3 },
                    new() { Input = "0 -1 1", ExpectedOutput = "1", IsSample = false, Order = 4 },
                }
            },
            new()
            {
                Title = "C. Even or Odd",
                Difficulty = "Easy",
                TimeLimitMs = 1000,
                IsActive = true,
                Order = 3,
                Statement =
@"Time limit: 1 second

You are given an integer n.
Print ""Even"" if n is even, otherwise print ""Odd"".

Input
A single integer n (−10^9 ≤ n ≤ 10^9).

Output
Print ""Even"" or ""Odd"" (without quotes).

Examples
Input
4
Output
Even

Input
7
Output
Odd",
                TestCases = new List<CodingTestCase>
                {
                    new() { Input = "4", ExpectedOutput = "Even", IsSample = true, Order = 1 },
                    new() { Input = "7", ExpectedOutput = "Odd", IsSample = true, Order = 2 },
                    new() { Input = "0", ExpectedOutput = "Even", IsSample = false, Order = 3 },
                    new() { Input = "-3", ExpectedOutput = "Odd", IsSample = false, Order = 4 },
                    new() { Input = "-10", ExpectedOutput = "Even", IsSample = false, Order = 5 },
                }
            },
            new()
            {
                Title = "D. Factorial",
                Difficulty = "Medium",
                TimeLimitMs = 2000,
                IsActive = true,
                Order = 4,
                Statement =
@"Time limit: 2 seconds

You are given a non-negative integer n.
Print n! (n factorial).

Input
A single integer n (0 ≤ n ≤ 12).

Output
Print a single integer — n!.

Examples
Input
5
Output
120

Input
0
Output
1",
                TestCases = new List<CodingTestCase>
                {
                    new() { Input = "5", ExpectedOutput = "120", IsSample = true, Order = 1 },
                    new() { Input = "0", ExpectedOutput = "1", IsSample = true, Order = 2 },
                    new() { Input = "1", ExpectedOutput = "1", IsSample = false, Order = 3 },
                    new() { Input = "10", ExpectedOutput = "3628800", IsSample = false, Order = 4 },
                    new() { Input = "12", ExpectedOutput = "479001600", IsSample = false, Order = 5 },
                }
            },
        };

        db.CodingProblems.AddRange(problems);
        await db.SaveChangesAsync();
    }
}
