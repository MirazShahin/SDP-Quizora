namespace Quizora.Application.DTOs.Code;

public class CodeRunRequestDto
{
    public string Language { get; set; } = "cpp";
    public string SourceCode { get; set; } = "";
    public string? Stdin { get; set; }
    public string? ExpectedOutput { get; set; }
}

public class CodeRunResultDto
{
    public bool Success { get; set; }
    public bool Compiled { get; set; }
    public bool TimedOut { get; set; }
    public bool Passed { get; set; }
    public string Status { get; set; } = "";
    public string Stdout { get; set; } = "";
    public string Stderr { get; set; } = "";
    public string CompileOutput { get; set; } = "";
    public int ExitCode { get; set; }
    public long TimeMs { get; set; }
}