using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PointRead.Services;

public static class IconFactory
{
    public static Icon CreateTrayIcon()
    {
        using var bitmap = CreateBitmap(32);
        return Icon.FromHandle(bitmap.GetHicon());
    }

    public static ImageSource CreateWindowIcon()
    {
        using var bitmap = CreateBitmap(64);
        var handle = bitmap.GetHbitmap();
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            handle,
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromWidthAndHeight(64, 64));
    }

    private static Bitmap CreateBitmap(int size)
    {
        var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(System.Drawing.Color.Transparent);

        using var background = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Rectangle(0, 0, size, size),
            System.Drawing.Color.FromArgb(37, 99, 235),
            System.Drawing.Color.FromArgb(20, 184, 166),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillRoundedRectangle(background, 2, 2, size - 4, size - 4, size / 5);

        using var pen = new System.Drawing.Pen(System.Drawing.Color.White, Math.Max(2, size / 11));
        graphics.DrawArc(pen, size * 0.23f, size * 0.22f, size * 0.54f, size * 0.42f, 200, 140);
        graphics.DrawLine(pen, size * 0.30f, size * 0.64f, size * 0.70f, size * 0.64f);
        graphics.DrawLine(pen, size * 0.50f, size * 0.65f, size * 0.50f, size * 0.80f);

        return bitmap;
    }

    private static void FillRoundedRectangle(this Graphics graphics, System.Drawing.Brush brush, float x, float y, float width, float height, float radius)
    {
        using var path = new GraphicsPath();
        path.AddArc(x, y, radius, radius, 180, 90);
        path.AddArc(x + width - radius, y, radius, radius, 270, 90);
        path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
        path.AddArc(x, y + height - radius, radius, radius, 90, 90);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }
}
