namespace Quizora.Web.Services;

/// <summary>
/// Maps raw HTTP / network failures into short user-friendly messages.
/// </summary>
public static class ApiErrors
{
    public static string Friendly(string? raw, string fallback = "Something went wrong. Please try again.")
    {
        if (string.IsNullOrWhiteSpace(raw))
            return fallback;

        var t = raw.Trim();
        var lower = t.ToLowerInvariant();

        if (lower.Contains("service suspended") || lower.Contains("serviceunavailable"))
            return "Server is temporarily unavailable (service suspended or sleeping). Please wait 1–2 minutes and try again.";

        if (lower.Contains("connection refused")
            || lower.Contains("actively refused")
            || lower.Contains("no connection could be made")
            || lower.Contains("failed to fetch")
            || lower.Contains("name or service not known")
            || lower.Contains("nodename nor servname")
            || lower.Contains("network is unreachable"))
            return "Cannot reach the server. Check your internet connection or try again later.";

        if (lower.Contains("timed out") || lower.Contains("timeout") || lower.Contains("taskcanceled"))
            return "The server is taking too long to respond. It may be waking up — please retry in a moment.";

        if (lower.Contains("<!doctype html>") || lower.Contains("<html"))
            return "Server returned an unexpected page instead of data. The API may be offline or misconfigured.";

        if (lower.Contains("500") || lower.Contains("internal server error"))
            return "Server error. Please try again later.";

        if (lower.Contains("401") || lower.Contains("unauthorized"))
            return "Session expired or unauthorized. Please log in again.";

        // Keep short API messages; trim huge HTML dumps
        if (t.Length > 220)
            t = t[..220] + "…";

        return t;
    }
}
