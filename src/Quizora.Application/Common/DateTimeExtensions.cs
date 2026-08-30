namespace Quizora.Application.Common;

public static class DateTimeExtensions
{
    public static DateTime ToUtcFromClientLocal(this DateTime local, int offsetMinutes)
    {
        var utc = local.AddMinutes(-offsetMinutes);
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc);
    }
}