namespace PointRead.Models;

public sealed class AppSettings
{
    public string HotkeyKey { get; set; } = "Q";
    public string SelectionReadHotkeyKey { get; set; } = "E";
    public string ClipboardReadHotkeyKey { get; set; } = "W";
    public string PlayPauseHotkeyKey { get; set; } = "P";
    public string PreviousSentenceHotkeyKey { get; set; } = "Left";
    public string NextSentenceHotkeyKey { get; set; } = "Right";
    public string StopHotkeyKey { get; set; } = "S";
    public int SpeechRate { get; set; }
    public double CaptureScaleAdjustment { get; set; } = 1.0;
}
