namespace Quizora.Application.DTOs.Attempts;

public class CheatSummaryDto
{
    public int TabSwitches { get; set; }
    public int FocusLost { get; set; }
    public int PasteAttempts { get; set; }
    public int CopyAttempts { get; set; }
}
