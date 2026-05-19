using System.Drawing;
using System.Windows;
using System.Windows.Media;

namespace PointRead.Services;

public sealed class ScreenCaptureService
{
    public Bitmap Capture(Rect selection, DpiScale dpi, double scaleAdjustment)
    {
        var scaleX = dpi.DpiScaleX * scaleAdjustment;
        var scaleY = dpi.DpiScaleY * scaleAdjustment;
        var left = (int)Math.Round(selection.Left * scaleX);
        var top = (int)Math.Round(selection.Top * scaleY);
        var width = Math.Max(1, (int)Math.Round(selection.Width * scaleX));
        var height = Math.Max(1, (int)Math.Round(selection.Height * scaleY));

        var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));
        return bitmap;
    }
}
