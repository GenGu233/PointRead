using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PointRead;

public partial class SelectionOverlayWindow : Window
{
    private System.Windows.Point? _startPoint;

    public event EventHandler<Rect>? SelectionCompleted;
    public event EventHandler? SelectionCanceled;

    public SelectionOverlayWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            Focus();
            Keyboard.Focus(this);
        };
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(this);
        SelectionRectangle.Visibility = Visibility.Visible;
        CaptureMouse();
    }

    private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_startPoint is null)
        {
            return;
        }

        var currentPoint = e.GetPosition(this);
        var rect = CreateRect(_startPoint.Value, currentPoint);

        Canvas.SetLeft(SelectionRectangle, rect.X);
        Canvas.SetTop(SelectionRectangle, rect.Y);
        SelectionRectangle.Width = rect.Width;
        SelectionRectangle.Height = rect.Height;
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_startPoint is null)
        {
            return;
        }

        var endPoint = e.GetPosition(this);
        var rect = CreateRect(_startPoint.Value, endPoint);

        ReleaseMouseCapture();
        Close();

        if (rect.Width >= 4 && rect.Height >= 4)
        {
            SelectionCompleted?.Invoke(this, rect);
        }
        else
        {
            SelectionCanceled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            SelectionCanceled?.Invoke(this, EventArgs.Empty);
        }
    }

    private static Rect CreateRect(System.Windows.Point start, System.Windows.Point end)
    {
        return new Rect(
            new System.Windows.Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y)),
            new System.Windows.Point(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y)));
    }
}
