using System.Diagnostics;
using System.Text;
using Quizora.Application.DTOs.Code;
using Quizora.Application.Interfaces;

namespace Quizora.Infrastructure.Services;

/// <summary>
/// Own C/C++ runner — no Judge0 / external API.
/// Needs gcc/g++ on PATH (Windows: MSYS2/MinGW, Linux: apt install g++).
/// Provides bits/stdc++.h shim so Codeforces-style includes work on minimal hosts.
/// </summary>
public class CodeExecutionService : ICodeExecutionService
{
    private static readonly TimeSpan CompileTimeout = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan RunTimeout = TimeSpan.FromSeconds(3);
    private const int MaxSourceBytes = 50_000;
    private const int MaxOutputChars = 20_000;

    /// <summary>Shim when system has no bits/stdc++.h (e.g. some Docker images).</summary>
    private const string StdCppShim =
        "#pragma once\n" +
        "#include <iostream>\n" +
        "#include <iomanip>\n" +
        "#include <sstream>\n" +
        "#include <fstream>\n" +
        "#include <string>\n" +
        "#include <vector>\n" +
        "#include <array>\n" +
        "#include <deque>\n" +
        "#include <queue>\n" +
        "#include <stack>\n" +
        "#include <list>\n" +
        "#include <map>\n" +
        "#include <unordered_map>\n" +
        "#include <set>\n" +
        "#include <unordered_set>\n" +
        "#include <algorithm>\n" +
        "#include <numeric>\n" +
        "#include <cmath>\n" +
        "#include <cstdlib>\n" +
        "#include <cstring>\n" +
        "#include <ctime>\n" +
        "#include <climits>\n" +
        "#include <cctype>\n" +
        "#include <bitset>\n" +
        "#include <utility>\n" +
        "#include <tuple>\n" +
        "#include <functional>\n";

    public async Task<CodeRunResultDto> RunAsync(CodeRunRequestDto request, CancellationToken ct = default)
    {
        var result = new CodeRunResultDto();

        if (request == null || string.IsNullOrWhiteSpace(request.SourceCode))
        {
            result.Status = "No source code";
            result.Stderr = "Source code is required";
            return result;
        }

        if (Encoding.UTF8.GetByteCount(request.SourceCode) > MaxSourceBytes)
        {
            result.Status = "Source too large";
            result.Stderr = $"Max source size is {MaxSourceBytes} bytes";
            return result;
        }

        var lang = (request.Language ?? "cpp").Trim().ToLowerInvariant();
        if (lang is not ("c" or "cpp" or "c++"))
        {
            result.Status = "Unsupported language";
            result.Stderr = "Only c or cpp allowed";
            return result;
        }

        var isCpp = lang is "cpp" or "c++";
        var work = Path.Combine(Path.GetTempPath(), "quizora-code", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(work);

        var sourceFile = Path.Combine(work, isCpp ? "main.cpp" : "main.c");
        var exeFile = Path.Combine(work, OperatingSystem.IsWindows() ? "main.exe" : "main");

        try
        {
            await File.WriteAllTextAsync(sourceFile, request.SourceCode, ct);

            // bits/stdc++.h shim for Codeforces-style code
            if (isCpp)
            {
                var bitsDir = Path.Combine(work, "bits");
                Directory.CreateDirectory(bitsDir);
                await File.WriteAllTextAsync(Path.Combine(bitsDir, "stdc++.h"), StdCppShim, ct);
            }

            var compiler = isCpp
                ? ResolveCompiler("g++", "g++.exe")
                : ResolveCompiler("gcc", "gcc.exe");

            if (compiler == null)
            {
                result.Status = "Compiler not found";
                result.Stderr = isCpp
                    ? "g++ not found. Install MSYS2/MinGW (Windows) or: sudo apt install g++ (Linux)"
                    : "gcc not found. Install MSYS2/MinGW or: sudo apt install gcc";
                return result;
            }

            // -I work so #include <bits/stdc++.h> finds work/bits/stdc++.h
            var compileArgs = isCpp
                ? $"-O2 -std=c++17 -pipe -I\"{work}\" \"{sourceFile}\" -o \"{exeFile}\""
                : $"-O2 -std=c11 -pipe \"{sourceFile}\" -o \"{exeFile}\"";

            var compile = await RunProcessAsync(compiler, compileArgs, work, null, CompileTimeout, ct);
            result.CompileOutput = Trim(compile.StdErr + compile.StdOut);
            result.Compiled = compile.ExitCode == 0 && File.Exists(exeFile);

            if (!result.Compiled)
            {
                result.Status = "Compilation Error";
                result.Stderr = result.CompileOutput;
                result.ExitCode = compile.ExitCode;
                return result;
            }

            var sw = Stopwatch.StartNew();
            var run = await RunProcessAsync(exeFile, "", work, request.Stdin ?? "", RunTimeout, ct);
            sw.Stop();

            result.TimeMs = sw.ElapsedMilliseconds;
            result.TimedOut = run.TimedOut;
            result.ExitCode = run.ExitCode;
            result.Stdout = Trim(run.StdOut);
            result.Stderr = Trim(run.StdErr);

            if (run.TimedOut)
            {
                result.Status = "Time Limit Exceeded";
                return result;
            }

            if (run.ExitCode != 0)
            {
                result.Success = true;
                result.Status = "Runtime Error";
                return result;
            }

            result.Success = true;
            result.Status = "Finished";

            if (!string.IsNullOrWhiteSpace(request.ExpectedOutput))
            {
                var got = Normalize(result.Stdout);
                var exp = Normalize(request.ExpectedOutput);
                result.Passed = got == exp;
                result.Status = result.Passed ? "Accepted" : "Wrong Answer";
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Status = "Engine Error";
            result.Stderr = ex.Message;
            return result;
        }
        finally
        {
            try
            {
                if (Directory.Exists(work))
                    Directory.Delete(work, recursive: true);
            }
            catch { /* ignore */ }
        }
    }

    private static string? ResolveCompiler(string unixName, string winName)
    {
        var fromPath = FindOnPath(OperatingSystem.IsWindows() ? winName : unixName)
                       ?? FindOnPath(unixName);
        if (fromPath != null)
            return fromPath;

        if (OperatingSystem.IsWindows())
        {
            var candidates = new[]
            {
                @"C:\msys64\ucrt64\bin\g++.exe",
                @"C:\msys64\mingw64\bin\g++.exe",
                @"C:\msys64\clang64\bin\g++.exe",
                @"C:\msys64\mingw32\bin\g++.exe",
                @"C:\MinGW\bin\g++.exe",
                @"C:\MinGW64\bin\g++.exe",
                @"C:\mingw64\bin\g++.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"msys64\ucrt64\bin\g++.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"msys64\mingw64\bin\g++.exe"),
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                    return path;
            }

            try
            {
                var whereResult = RunWhereCommand(winName);
                if (!string.IsNullOrWhiteSpace(whereResult) && File.Exists(whereResult))
                    return whereResult;
            }
            catch { }
        }

        return null;
    }

    private static string? FindOnPath(string fileName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var full = Path.Combine(dir.Trim().Trim('"'), fileName);
                if (File.Exists(full))
                    return full;
            }
            catch { }
        }
        return null;
    }

    private static string? RunWhereCommand(string fileName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = fileName,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            if (proc == null) return null;
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(2000);
            return output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()?.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static async Task<(int ExitCode, string StdOut, string StdErr, bool TimedOut)> RunProcessAsync(
        string fileName,
        string args,
        string workDir,
        string? stdin,
        TimeSpan timeout,
        CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            WorkingDirectory = workDir,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        proc.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        if (stdin != null)
        {
            try
            {
                await proc.StandardInput.WriteAsync(stdin);
                await proc.StandardInput.FlushAsync();
                proc.StandardInput.Close();
            }
            catch { }
        }
        else
        {
            try { proc.StandardInput.Close(); } catch { }
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            await proc.WaitForExitAsync(cts.Token);
            return (proc.ExitCode, stdout.ToString(), stderr.ToString(), false);
        }
        catch (OperationCanceledException)
        {
            try { if (!proc.HasExited) proc.Kill(entireProcessTree: true); } catch { }
            return (-1, stdout.ToString(), stderr.ToString(), true);
        }
    }

    private static string Normalize(string s)
        => string.Join("\n",
                s.Replace("\r\n", "\n").Replace('\r', '\n')
                    .Split('\n')
                    .Select(l => l.TrimEnd()))
            .Trim();

    private static string Trim(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length > MaxOutputChars)
            s = s[..MaxOutputChars] + "\n...[truncated]";
        return s.Trim();
    }
}