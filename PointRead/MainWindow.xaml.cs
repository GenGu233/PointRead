using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using PointRead.Models;
using PointRead.Services;
using Forms = System.Windows.Forms;

namespace PointRead;

public partial class MainWindow : Window
{
    private const int HotkeyId = 9000;
    private const int ClipboardReadHotkeyId = 9005;
    private const int SelectionReadHotkeyId = 9006;
    private const int PlayPauseHotkeyId = 9001;
    private const int PreviousSentenceHotkeyId = 9002;
    private const int NextSentenceHotkeyId = 9003;
    private const int StopHotkeyId = 9004;
    private const uint ModAlt = 0x0001;
    private HwndSource? _source;
    private readonly ScreenCaptureService _screenCaptureService = new();
    private readonly OcrService _ocrService = new();
    private readonly SpeechService _speechService = new();
    private readonly SettingsService _settingsService = new();
    private readonly ClipboardReadService _clipboardReadService = new();
    private readonly AutomationSelectionReadService _automationSelectionReadService = new();
    private readonly Forms.NotifyIcon _notifyIcon = new();
    private FloatingToolbarWindow? _floatingToolbarWindow;
    private AppSettings _settings;
    private bool _isExitRequested;

    public MainWindow()
    {
        InitializeComponent();
        Icon = IconFactory.CreateWindowIcon();
        _settings = _settingsService.Load();
        RateSlider.Value = _settings.SpeechRate;
        RateValueText.Text = FormatRate(_settings.SpeechRate);
        _speechService.Rate = _settings.SpeechRate;
        HotkeyHintText.Text = $"按 Alt + {_settings.HotkeyKey} 进入点读框选模式";
        ConfigureTrayIcon();
        SourceInitialized += MainWindow_SourceInitialized;
        Closed += MainWindow_Closed;
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        _source = HwndSource.FromHwnd(handle);
        _source.AddHook(HwndHook);

        var registered = RegisterConfiguredHotkeys(handle);
        StatusText.Text = registered
            ? $"已准备好。按 Alt + {_settings.HotkeyKey} 进入点读框选模式。"
            : "有快捷键注册失败，可能已被其他程序占用。";
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        UnregisterHotKey(handle, HotkeyId);
        UnregisterHotKey(handle, ClipboardReadHotkeyId);
        UnregisterHotKey(handle, SelectionReadHotkeyId);
        UnregisterHotKey(handle, PlayPauseHotkeyId);
        UnregisterHotKey(handle, PreviousSentenceHotkeyId);
        UnregisterHotKey(handle, NextSentenceHotkeyId);
        UnregisterHotKey(handle, StopHotkeyId);
        _source?.RemoveHook(HwndHook);
        _speechService.Dispose();
        _floatingToolbarWindow?.Close();
        _floatingToolbarWindow = null;
        _notifyIcon.Dispose();
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WmHotkey = 0x0312;

        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            BeginSelection();
            handled = true;
        }
        else if (msg == WmHotkey && wParam.ToInt32() == ClipboardReadHotkeyId)
        {
            BeginClipboardRead();
            handled = true;
        }
        else if (msg == WmHotkey && wParam.ToInt32() == SelectionReadHotkeyId)
        {
            BeginAutomationSelectionRead();
            handled = true;
        }
        else if (msg == WmHotkey && wParam.ToInt32() == PlayPauseHotkeyId)
        {
            _speechService.TogglePause();
            handled = true;
        }
        else if (msg == WmHotkey && wParam.ToInt32() == PreviousSentenceHotkeyId)
        {
            _speechService.PreviousSegment();
            handled = true;
        }
        else if (msg == WmHotkey && wParam.ToInt32() == NextSentenceHotkeyId)
        {
            _speechService.NextSegment();
            handled = true;
        }
        else if (msg == WmHotkey && wParam.ToInt32() == StopHotkeyId)
        {
            _speechService.Stop();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void StartSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        BeginSelection();
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        _isExitRequested = true;
        Close();
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        _speechService.TogglePause();
    }

    private void PreviousSentenceButton_Click(object sender, RoutedEventArgs e)
    {
        _speechService.PreviousSegment();
    }

    private void NextSentenceButton_Click(object sender, RoutedEventArgs e)
    {
        _speechService.NextSegment();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _speechService.Stop();
    }

    private void RateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var rate = (int)e.NewValue;
        RateValueText.Text = FormatRate(rate);
        _speechService.Rate = rate;
        _settings.SpeechRate = rate;
        _settingsService.Save(_settings);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_settings)
        {
            Owner = this
        };

        if (settingsWindow.ShowDialog() != true)
        {
            return;
        }

        _settings = settingsWindow.Result;
        RateSlider.Value = _settings.SpeechRate;
        RateValueText.Text = FormatRate(_settings.SpeechRate);
        _speechService.Rate = _settings.SpeechRate;
        _settingsService.Save(_settings);
        RefreshHotkey();
        HotkeyHintText.Text = $"按 Alt + {_settings.HotkeyKey} 进入点读框选模式";
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isExitRequested)
        {
            return;
        }

        e.Cancel = true;
        Hide();
        StatusText.Text = "已隐藏到托盘。";
    }

    private void BeginSelection()
    {
        StatusText.Text = "正在框选。拖拽鼠标选择区域，按 Esc 取消。";

        var overlay = new SelectionOverlayWindow();
        overlay.SelectionCompleted += async (_, selection) =>
        {
            StatusText.Text = $"已选择区域：{selection.Width:0} x {selection.Height:0}。正在识别...";
            Activate();

            try
            {
                var dpi = VisualTreeHelper.GetDpi(overlay);
                using var screenshot = _screenCaptureService.Capture(selection, dpi, _settings.CaptureScaleAdjustment);
                var text = await _ocrService.RecognizeAsync(screenshot);
                RecognizedTextBox.Text = text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    StatusText.Text = "没有识别到文字。";
                    return;
                }

                StatusText.Text = $"识别完成，正在朗读。当前缩放：{dpi.DpiScaleX:0.##}x。";
                _speechService.Load(text);
                _speechService.PlayFromCurrentPosition();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"识别失败：{ex.Message}";
            }
        };
        overlay.SelectionCanceled += (_, _) =>
        {
            StatusText.Text = "已取消框选。";
            Activate();
        };
        overlay.Show();
    }

    private void BeginClipboardRead()
    {
        try
        {
            StatusText.Text = "正在读取剪贴板文本...";
            var text = _clipboardReadService.ReadClipboardText();
            RecognizedTextBox.Text = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                StatusText.Text = "剪贴板里没有可朗读的文字。请先复制，再按剪贴板朗读快捷键。";
                return;
            }

            StatusText.Text = "已读取剪贴板文本，正在朗读。";
            _speechService.Load(text);
            _speechService.PlayFromCurrentPosition();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"剪贴板朗读失败：{ex.Message}";
        }
    }

    private void BeginAutomationSelectionRead()
    {
        try
        {
            StatusText.Text = "正在读取当前选区...";
            var text = _automationSelectionReadService.ReadSelectedText();
            RecognizedTextBox.Text = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                StatusText.Text = "当前应用没有提供可读取的文本选区，可改用剪贴板朗读。";
                return;
            }

            StatusText.Text = "已读取当前选区，正在朗读。";
            _speechService.Load(text);
            _speechService.PlayFromCurrentPosition();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"选读失败：{ex.Message}";
        }
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private bool RegisterConfiguredHotkeys(IntPtr handle)
    {
        return RegisterHotKey(handle, HotkeyId, ModAlt, ToVirtualKey(_settings.HotkeyKey))
            && RegisterHotKey(handle, SelectionReadHotkeyId, ModAlt, ToVirtualKey(_settings.SelectionReadHotkeyKey))
            && RegisterHotKey(handle, ClipboardReadHotkeyId, ModAlt, ToVirtualKey(_settings.ClipboardReadHotkeyKey))
            && RegisterHotKey(handle, PlayPauseHotkeyId, ModAlt, ToVirtualKey(_settings.PlayPauseHotkeyKey))
            && RegisterHotKey(handle, PreviousSentenceHotkeyId, ModAlt, ToVirtualKey(_settings.PreviousSentenceHotkeyKey))
            && RegisterHotKey(handle, NextSentenceHotkeyId, ModAlt, ToVirtualKey(_settings.NextSentenceHotkeyKey))
            && RegisterHotKey(handle, StopHotkeyId, ModAlt, ToVirtualKey(_settings.StopHotkeyKey));
    }

    private void RefreshHotkey()
    {
        var handle = new WindowInteropHelper(this).Handle;
        UnregisterHotKey(handle, HotkeyId);
        UnregisterHotKey(handle, ClipboardReadHotkeyId);
        UnregisterHotKey(handle, SelectionReadHotkeyId);
        UnregisterHotKey(handle, PlayPauseHotkeyId);
        UnregisterHotKey(handle, PreviousSentenceHotkeyId);
        UnregisterHotKey(handle, NextSentenceHotkeyId);
        UnregisterHotKey(handle, StopHotkeyId);
        var registered = RegisterConfiguredHotkeys(handle);
        StatusText.Text = registered
            ? $"设置已保存。按 Alt + {_settings.HotkeyKey} 进入点读框选模式。"
            : "设置已保存，但有快捷键注册失败。";
    }

    private static uint ToVirtualKey(string key)
    {
        return key switch
        {
            "Left" => 0x25,
            "Up" => 0x26,
            "Right" => 0x27,
            "Down" => 0x28,
            _ => char.ToUpperInvariant(key[0])
        };
    }

    private static string FormatRate(int rate)
    {
        return $"{1 + (rate * 0.1):0.0}x";
    }

    private void ConfigureTrayIcon()
    {
        _notifyIcon.Icon = IconFactory.CreateTrayIcon();
        _notifyIcon.Text = "PointRead";
        _notifyIcon.Visible = true;
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("显示主窗口", null, (_, _) => ShowMainWindow());
        menu.Items.Add("开始框读", null, (_, _) => BeginSelection());
        menu.Items.Add("显示/隐藏悬浮窗", null, (_, _) => ToggleFloatingToolbar());
        menu.Items.Add("退出", null, (_, _) =>
        {
            _isExitRequested = true;
            Close();
        });
        _notifyIcon.ContextMenuStrip = menu;
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ToggleFloatingToolbar()
    {
        if (_floatingToolbarWindow is null)
        {
            _floatingToolbarWindow = new FloatingToolbarWindow();
            _floatingToolbarWindow.StartSelectionRequested += (_, _) => BeginSelection();
            _floatingToolbarWindow.ClipboardReadRequested += (_, _) => BeginClipboardRead();
            _floatingToolbarWindow.PlayPauseRequested += (_, _) => _speechService.TogglePause();
            _floatingToolbarWindow.PreviousRequested += (_, _) => _speechService.RewindSeconds(5);
            _floatingToolbarWindow.NextRequested += (_, _) => _speechService.ForwardSeconds(5);
            _floatingToolbarWindow.StopRequested += (_, _) => _speechService.Stop();
            _floatingToolbarWindow.HideRequested += (_, _) => _floatingToolbarWindow.Hide();
        }

        if (_floatingToolbarWindow.IsVisible)
        {
            _floatingToolbarWindow.Hide();
        }
        else
        {
            _floatingToolbarWindow.Show();
            _floatingToolbarWindow.Activate();
        }
    }
}
