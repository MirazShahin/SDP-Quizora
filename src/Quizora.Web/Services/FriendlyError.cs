using System.Net.Http.Json;

namespace Quizora.Web.Services;
 
public static class FriendlyError
{
    public const string ServerUnavailable =
        "We couldn't reach the Quizora server right now. Please check your connection and try again in a moment.";

    public const string TimedOut =
        "The server is taking too long to respond. Please try again.";

    public const string UnexpectedResponse =
        "Something unexpected happened while talking to the server. Please try again.";

    public static string Describe(Exception ex) => ex switch
    {
        HttpRequestException => ServerUnavailable,
        TaskCanceledException => TimedOut,
        OperationCanceledException => TimedOut,
        System.Text.Json.JsonException => UnexpectedResponse,
        NotSupportedException => UnexpectedResponse,
        _ => "Something went wrong. Please try again, and contact support if the problem continues."
    };
}
