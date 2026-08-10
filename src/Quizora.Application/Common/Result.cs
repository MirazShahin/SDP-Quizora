namespace Quizora.Application.Common;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public Result() { }

    public Result(bool isSuccess, string? message = null)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static Result Success(string? message = null) => new(true, message);
    public static Result Failure(string message) => new(false, message);
    public static Result Failure(List<string> errors) => new(false) { Errors = errors };
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public Result() { }

    public Result(bool isSuccess, T? data, string? message = null) : base(isSuccess, message)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string? message = null) => new(true, data, message);
    public static new Result<T> Failure(string message) => new(false, default, message);
}