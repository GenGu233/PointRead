using System.Windows;
using System.Windows.Input;

namespace PointRead;

public partial class FloatingToolbarWindow : Window
{
    public event EventHandler? StartSelectionRequested;
    public event EventHandler? ClipboardReadRequested;
    public event EventHandler? PlayPauseRequested;
    public event EventHandler? PreviousRequested;
    public event EventHandler? NextRequested;
    public event EventHandler? StopRequested;
    public event EventHandler? HideRequested;

    public FloatingToolbarWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void StartSelectionButton_Click(object sender, RoutedEventArgs e) =>
        StartSelectionRequested?.Invoke(this, EventArgs.Empty);

    private void ClipboardReadButton_Click(object sender, RoutedEventArgs e) =>
        ClipboardReadRequested?.Invoke(this, EventArgs.Empty);

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e) =>
        PlayPauseRequested?.Invoke(this, EventArgs.Empty);

    private void PreviousButton_Click(object sender, RoutedEventArgs e) =>
        PreviousRequested?.Invoke(this, EventArgs.Empty);

    private void NextButton_Click(object sender, RoutedEventArgs e) =>
        NextRequested?.Invoke(this, EventArgs.Empty);

    private void StopButton_Click(object sender, RoutedEventArgs e) =>
        StopRequested?.Invoke(this, EventArgs.Empty);

    private void HideButton_Click(object sender, RoutedEventArgs e) =>
        HideRequested?.Invoke(this, EventArgs.Empty);
}
