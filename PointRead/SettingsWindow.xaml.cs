using System.Windows;
using PointRead.Models;

namespace PointRead;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public AppSettings Result => _settings;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = new AppSettings
        {
            HotkeyKey = settings.HotkeyKey,
            SelectionReadHotkeyKey = settings.SelectionReadHotkeyKey,
            ClipboardReadHotkeyKey = settings.ClipboardReadHotkeyKey,
            PlayPauseHotkeyKey = settings.PlayPauseHotkeyKey,
            PreviousSentenceHotkeyKey = settings.PreviousSentenceHotkeyKey,
            NextSentenceHotkeyKey = settings.NextSentenceHotkeyKey,
            StopHotkeyKey = settings.StopHotkeyKey,
            SpeechRate = settings.SpeechRate,
            CaptureScaleAdjustment = settings.CaptureScaleAdjustment
        };

        var letterOptions = Enumerable.Range('A', 26).Select(code => ((char)code).ToString()).ToList();
        var playbackOptions = letterOptions.Concat(["Left", "Right", "Up", "Down"]).ToList();

        HotkeyComboBox.ItemsSource = letterOptions;
        HotkeyComboBox.SelectedItem = _settings.HotkeyKey;
        SelectionReadComboBox.ItemsSource = letterOptions;
        SelectionReadComboBox.SelectedItem = _settings.SelectionReadHotkeyKey;
        ClipboardReadComboBox.ItemsSource = letterOptions;
        ClipboardReadComboBox.SelectedItem = _settings.ClipboardReadHotkeyKey;
        PlayPauseComboBox.ItemsSource = playbackOptions;
        PlayPauseComboBox.SelectedItem = _settings.PlayPauseHotkeyKey;
        PreviousComboBox.ItemsSource = playbackOptions;
        PreviousComboBox.SelectedItem = _settings.PreviousSentenceHotkeyKey;
        NextComboBox.ItemsSource = playbackOptions;
        NextComboBox.SelectedItem = _settings.NextSentenceHotkeyKey;
        StopComboBox.ItemsSource = playbackOptions;
        StopComboBox.SelectedItem = _settings.StopHotkeyKey;
        RateSlider.Value = _settings.SpeechRate;
        RateValueText.Text = FormatRate(_settings.SpeechRate);
        CaptureScaleSlider.Value = _settings.CaptureScaleAdjustment;
        CaptureScaleValueText.Text = FormatScale(_settings.CaptureScaleAdjustment);
    }

    private void RateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
        {
            return;
        }

        RateValueText.Text = FormatRate((int)e.NewValue);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void CaptureScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
        {
            return;
        }

        CaptureScaleValueText.Text = FormatScale(e.NewValue);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.HotkeyKey = HotkeyComboBox.SelectedItem?.ToString() ?? "Q";
        _settings.SelectionReadHotkeyKey = SelectionReadComboBox.SelectedItem?.ToString() ?? "E";
        _settings.ClipboardReadHotkeyKey = ClipboardReadComboBox.SelectedItem?.ToString() ?? "W";
        _settings.PlayPauseHotkeyKey = PlayPauseComboBox.SelectedItem?.ToString() ?? "P";
        _settings.PreviousSentenceHotkeyKey = PreviousComboBox.SelectedItem?.ToString() ?? "Left";
        _settings.NextSentenceHotkeyKey = NextComboBox.SelectedItem?.ToString() ?? "Right";
        _settings.StopHotkeyKey = StopComboBox.SelectedItem?.ToString() ?? "S";
        _settings.SpeechRate = (int)RateSlider.Value;
        _settings.CaptureScaleAdjustment = Math.Round(CaptureScaleSlider.Value, 2);
        DialogResult = true;
    }

    private static string FormatRate(int rate)
    {
        return $"{1 + (rate * 0.1):0.0}x";
    }

    private static string FormatScale(double scale)
    {
        return $"{scale:0.00}x";
    }
}
