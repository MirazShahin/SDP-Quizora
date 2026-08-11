public class WeakTopicDto
{
    public string Topic { get; set; } = "";
    public int Score { get; set; }
    public int Total { get; set; }
    public double Accuracy { get; set; }      // 0–100
    public string Severity { get; set; } = ""; // High / Medium / Low
    public string Source { get; set; } = "";  // Practice / Mock
}