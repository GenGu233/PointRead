using System.Text.RegularExpressions;

namespace PointRead.Services;

public sealed class SpeechService
{
    private const int SpeakAsync = 1;
    private const int SpeakPurgeBeforeSpeak = 2;

    private readonly dynamic _voice = Activator.CreateInstance(Type.GetTypeFromProgID("SAPI.SpVoice")!)!;
    private readonly List<string> _segments = [];
    private string _currentText = string.Empty;
    private int _currentCharacterIndex;
    private bool _isPaused;

    public int Rate
    {
        get => _voice.Rate;
        set => _voice.Rate = Math.Clamp(value, -10, 10);
    }

    public void Load(string text)
    {
        Stop();
        _segments.Clear();
        _segments.AddRange(SplitSegments(text));
        _currentText = string.Join(Environment.NewLine, _segments);
        _currentCharacterIndex = 0;
        _isPaused = false;
    }

    public void PlayFromCurrentPosition()
    {
        if (string.IsNullOrWhiteSpace(_currentText))
        {
            return;
        }

        var remainingText = _currentText[Math.Min(_currentCharacterIndex, _currentText.Length)..];
        _voice.Speak(remainingText, SpeakAsync | SpeakPurgeBeforeSpeak);
        _isPaused = false;
    }

    public void PlayFromCurrentSegment() => PlayFromCurrentPosition();

    public void TogglePause()
    {
        if (_isPaused)
        {
            _voice.Resume();
            _isPaused = false;
        }
        else
        {
            _voice.Pause();
            _isPaused = true;
        }
    }

    public void RewindSeconds(int seconds)
    {
        if (string.IsNullOrWhiteSpace(_currentText))
        {
            return;
        }

        _currentCharacterIndex = Math.Max(0, _currentCharacterIndex - EstimateCharacters(seconds));
        PlayFromCurrentPosition();
    }

    public void ForwardSeconds(int seconds)
    {
        if (string.IsNullOrWhiteSpace(_currentText))
        {
            return;
        }

        _currentCharacterIndex = Math.Min(_currentText.Length, _currentCharacterIndex + EstimateCharacters(seconds));
        PlayFromCurrentPosition();
    }

    public void PreviousSegment() => RewindSeconds(5);

    public void NextSegment() => ForwardSeconds(5);

    public void Stop()
    {
        if (_isPaused)
        {
            _voice.Resume();
        }

        _voice.Speak(string.Empty, SpeakAsync | SpeakPurgeBeforeSpeak);
        _isPaused = false;
    }

    public void Dispose()
    {
        Stop();
    }

    private static IEnumerable<string> SplitSegments(string text)
    {
        var normalized = text.Replace("\r\n", "\n").Trim();
        var roughSegments = Regex.Split(normalized, @"(?<=[。！？.!?])\s*|\n+")
            .Select(segment => segment.Trim())
            .Where(segment => !string.IsNullOrWhiteSpace(segment));

        foreach (var segment in roughSegments)
        {
            if (segment.Length <= 60)
            {
                yield return segment;
                continue;
            }

            for (var index = 0; index < segment.Length; index += 60)
            {
                yield return segment.Substring(index, Math.Min(60, segment.Length - index));
            }
        }
    }

    private static int EstimateCharacters(int seconds)
    {
        return Math.Max(20, seconds * 12);
    }
}
