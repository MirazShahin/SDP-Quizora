namespace Quizora.Web.Services;

 
public static class ClientTimeExtensions
{
    
    public static DateTime ToClientLocal(this DateTime utc, int? offsetMinutes)
    {
        var asUtc = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return offsetMinutes.HasValue ? asUtc.AddMinutes(offsetMinutes.Value) : asUtc;
    }
     
    public static DateTime ToUtcFromClientLocal(this DateTime wallClock, int offsetMinutes)
    {
        var asUtc = DateTime.SpecifyKind(wallClock, DateTimeKind.Utc);
        return asUtc.AddMinutes(-offsetMinutes);
    }
}