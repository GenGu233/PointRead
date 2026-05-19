using System.Windows;

namespace PointRead.Services;

public sealed class ClipboardReadService
{
    public string ReadClipboardText()
    {
        return System.Windows.Clipboard.ContainsText()
            ? System.Windows.Clipboard.GetText().Trim()
            : string.Empty;
    }
}
