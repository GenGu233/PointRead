using System.Windows.Automation;

namespace PointRead.Services;

public sealed class AutomationSelectionReadService
{
    public string ReadSelectedText()
    {
        var focusedElement = AutomationElement.FocusedElement;
        if (focusedElement is null)
        {
            return string.Empty;
        }

        if (!focusedElement.TryGetCurrentPattern(TextPattern.Pattern, out var patternObject))
        {
            return string.Empty;
        }

        var textPattern = (TextPattern)patternObject;
        var selectedRanges = textPattern.GetSelection();
        if (selectedRanges.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(
            Environment.NewLine,
            selectedRanges
                .Select(range => range.GetText(-1).Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text)));
    }
}
