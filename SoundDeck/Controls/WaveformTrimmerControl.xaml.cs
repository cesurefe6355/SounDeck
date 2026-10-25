using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SoundDeck.Services;

namespace SoundDeck.Controls;

public partial class WaveformTrimmerControl : UserControl
{
    private enum DragMode
    {
        None,
        LeftHandle,
        RightHandle
    }

    private readonly WaveformCanvas _canvas = new();
    private DragMode _dragMode = DragMode.None;
    private double _totalSeconds = 0;
    private double _startSeconds = 0;
    private double _endSeconds = 0;
    private double? _playheadSeconds;
    private float[] _peaks = Array.Empty<float>();
    private bool _isPlaying;

    public WaveformTrimmerControl()
    {
        InitializeComponent();

        OverlayCanvas.Children.Add(_canvas);
        _canvas.MouseDown += OnCanvasMouseDown;
        _canvas.MouseMove += OnCanvasMouseMove;
        _canvas.MouseUp += OnCanvasMouseUp;
        _canvas.MouseLeave += OnCanvasMouseLeave;

        SizeChanged += (_, _) => UpdateCanvasSize();
    }

    public event EventHandler<(double Start, double End)>? RangeChanged;

    public event EventHandler<double>? SeekRequested;

    public event EventHandler? PlayToggleRequested;

    public double TotalSeconds => _totalSeconds;

    public double StartSeconds
    {
        get => _startSeconds;
        set
        {
            if (_totalSeconds <= 0) return;
            var clamped = Math.Clamp(value, 0, _totalSeconds);
            if (Math.Abs(_startSeconds - clamped) > 0.001)
            {
                _startSeconds = clamped;
                if (_startSeconds > _endSeconds)
                {
                    _endSeconds = _startSeconds;
                }
                UpdateRatios();
            }
        }
    }

    public double EndSeconds
    {
        get => _endSeconds;
        set
        {
            if (_totalSeconds <= 0) return;
            var clamped = Math.Clamp(value, 0, _totalSeconds);
            if (Math.Abs(_endSeconds - clamped) > 0.001)
            {
                _endSeconds = clamped;
                if (_endSeconds < _startSeconds)
                {
                    _startSeconds = _endSeconds;
                }
                UpdateRatios();
            }
        }
    }

    public void LoadAudio(string filePath, double initialStart = 0, double? initialEnd = null)
    {
        var duration = AudioEditorService.GetDuration(filePath);
        _totalSeconds = duration.TotalSeconds > 0 ? duration.TotalSeconds : 0;

        if (_totalSeconds > 0)
        {
            _startSeconds = Math.Clamp(initialStart, 0, _totalSeconds);
            _endSeconds = initialEnd.HasValue
                ? Math.Clamp(initialEnd.Value, _startSeconds, _totalSeconds)
                : _totalSeconds;
        }
        else
        {
            _startSeconds = 0;
            _endSeconds = 0;
        }

        // Asenkron veya hızlı pik çıkarma
        _peaks = AudioEditorService.ExtractPeaks(filePath, 600);

        _canvas.SetPeaks(_peaks);
        UpdateRatios();
    }

    public void SetPlaybackPosition(double? seconds)
    {
        _playheadSeconds = seconds;
        if (seconds.HasValue && _totalSeconds > 0)
        {
            _canvas.PlayheadRatio = Math.Clamp(seconds.Value / _totalSeconds, 0, 1);
        }
        else
        {
            _canvas.PlayheadRatio = null;
        }
    }

    public void SetPlayingState(bool isPlaying)
    {
        _isPlaying = isPlaying;
        if (PlayButton.Template?.FindName("BtnIcon", PlayButton) is TextBlock iconBlock)
        {
            iconBlock.Text = isPlaying ? "\uE769" : "\uE768"; // Pause / Play icon
            iconBlock.Margin = isPlaying ? new Thickness(0) : new Thickness(2, 0, 0, 0);
        }
    }

    public void ResetTrim()
    {
        if (_totalSeconds <= 0) return;
        _startSeconds = 0;
        _endSeconds = _totalSeconds;
        UpdateRatios();
        RangeChanged?.Invoke(this, (_startSeconds, _endSeconds));
    }

    private void UpdateCanvasSize()
    {
        _canvas.Width = Math.Max(0, WaveformBorder.ActualWidth - 2);
        _canvas.Height = Math.Max(0, WaveformBorder.ActualHeight - 2);
    }

    private void UpdateRatios()
    {
        if (_totalSeconds > 0)
        {
            _canvas.StartRatio = Math.Clamp(_startSeconds / _totalSeconds, 0, 1);
            _canvas.EndRatio = Math.Clamp(_endSeconds / _totalSeconds, 0, 1);
        }
        else
        {
            _canvas.StartRatio = 0;
            _canvas.EndRatio = 1;
        }

        _canvas.InvalidateVisual();
    }

    private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_totalSeconds <= 0 || e.ChangedButton != MouseButton.Left) return;

        var pos = e.GetPosition(_canvas);
        var width = _canvas.ActualWidth;
        if (width <= 0) return;

        var startX = _canvas.StartRatio * width;
        var endX = _canvas.EndRatio * width;

        if (Math.Abs(pos.X - startX) <= 14)
        {
            _dragMode = DragMode.LeftHandle;
            _canvas.CaptureMouse();
            ShowBadge(pos.X, _startSeconds);
        }
        else if (Math.Abs(pos.X - endX) <= 14)
        {
            _dragMode = DragMode.RightHandle;
            _canvas.CaptureMouse();
            ShowBadge(pos.X, _endSeconds);
        }
        else if (pos.X >= startX && pos.X <= endX)
        {
            // Tıklanan yere playhead taşı
            var seekSec = (pos.X / width) * _totalSeconds;
            SeekRequested?.Invoke(this, seekSec);
        }
    }

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(_canvas);
        var width = _canvas.ActualWidth;
        if (width <= 0 || _totalSeconds <= 0) return;

        var startX = _canvas.StartRatio * width;
        var endX = _canvas.EndRatio * width;

        if (_dragMode == DragMode.LeftHandle)
        {
            var newRatio = Math.Clamp(pos.X / width, 0, _canvas.EndRatio - 0.005);
            _canvas.StartRatio = newRatio;
            _startSeconds = newRatio * _totalSeconds;
            _canvas.InvalidateVisual();
            ShowBadge(newRatio * width, _startSeconds);
            RangeChanged?.Invoke(this, (_startSeconds, _endSeconds));
            return;
        }

        if (_dragMode == DragMode.RightHandle)
        {
            var newRatio = Math.Clamp(pos.X / width, _canvas.StartRatio + 0.005, 1);
            _canvas.EndRatio = newRatio;
            _endSeconds = newRatio * _totalSeconds;
            _canvas.InvalidateVisual();
            ShowBadge(newRatio * width, _endSeconds);
            RangeChanged?.Invoke(this, (_startSeconds, _endSeconds));
            return;
        }

        // Hover cursor
        if (Math.Abs(pos.X - startX) <= 14 || Math.Abs(pos.X - endX) <= 14)
        {
            _canvas.Cursor = Cursors.SizeWE;
        }
        else if (pos.X > startX && pos.X < endX)
        {
            _canvas.Cursor = Cursors.Hand;
        }
        else
        {
            _canvas.Cursor = Cursors.Arrow;
        }
    }

    private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_dragMode != DragMode.None)
        {
            _dragMode = DragMode.None;
            _canvas.ReleaseMouseCapture();
            TimeBadge.Visibility = Visibility.Collapsed;
        }
    }

    private void OnCanvasMouseLeave(object sender, MouseEventArgs e)
    {
        if (_dragMode == DragMode.None)
        {
            TimeBadge.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowBadge(double x, double seconds)
    {
        TimeBadgeText.Text = FormatBadgeTime(seconds);
        TimeBadge.Visibility = Visibility.Visible;

        // Position badge above handle
        var badgeLeft = Math.Clamp(x - 26, 4, Math.Max(4, _canvas.ActualWidth - 65));
        Canvas.SetLeft(TimeBadge, badgeLeft);
        Canvas.SetTop(TimeBadge, 6);
    }

    private static string FormatBadgeTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss\.ff", CultureInfo.InvariantCulture)
            : ts.ToString(@"mm\:ss\.ff", CultureInfo.InvariantCulture);
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        PlayToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private sealed class WaveformCanvas : FrameworkElement
    {
        private readonly Pen _mutedWavePen = new(new SolidColorBrush(Color.FromRgb(0x40, 0x47, 0x57)), 2.2)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };

        private readonly Pen _activeWavePen = new(new SolidColorBrush(Color.FromRgb(0x00, 0x84, 0xFF)), 2.2)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };

        private readonly Brush _dimBrush = new SolidColorBrush(Color.FromArgb(170, 8, 10, 14));
        private readonly Brush _yellowHandleBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC8, 0x20));
        private readonly Brush _handleNotchBrush = new SolidColorBrush(Color.FromRgb(0x40, 0x30, 0x00));
        private readonly Pen _yellowFramePen = new(new SolidColorBrush(Color.FromRgb(0xFF, 0xC8, 0x20)), 2.5);
        private readonly Pen _playheadPen = new(new SolidColorBrush(Color.FromArgb(240, 255, 255, 255)), 1.5);

        private float[] _peaks = Array.Empty<float>();

        public WaveformCanvas()
        {
            _mutedWavePen.Freeze();
            _activeWavePen.Freeze();
            _dimBrush.Freeze();
            _yellowHandleBrush.Freeze();
            _handleNotchBrush.Freeze();
            _yellowFramePen.Freeze();
            _playheadPen.Freeze();
        }

        public double StartRatio { get; set; } = 0;

        public double EndRatio { get; set; } = 1;

        public double? PlayheadRatio { get; set; }

        public void SetPeaks(float[] peaks)
        {
            _peaks = peaks;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var w = ActualWidth;
            var h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            var centerY = h / 2.0;

            // 1. Dalga Formu Çizimi
            if (_peaks.Length > 0)
            {
                var step = 3.5;
                var count = (int)(w / step);

                for (int i = 0; i < count; i++)
                {
                    var x = i * step + 2;
                    var ratio = x / w;

                    var peakIndex = Math.Clamp((int)(ratio * _peaks.Length), 0, _peaks.Length - 1);
                    var amp = _peaks[peakIndex];
                    var barH = Math.Max(3.0, amp * (h * 0.82));
                    var top = centerY - (barH / 2.0);
                    var bottom = centerY + (barH / 2.0);

                    var isInside = ratio >= StartRatio && ratio <= EndRatio;
                    var pen = isInside ? _activeWavePen : _mutedWavePen;

                    dc.DrawLine(pen, new Point(x, top), new Point(x, bottom));
                }
            }
            else
            {
                // Pikler henüz yüklenmediyse ortada yumuşak bir çizgi
                dc.DrawLine(_mutedWavePen, new Point(0, centerY), new Point(w, centerY));
            }

            var startX = Math.Clamp(StartRatio * w, 0, w);
            var endX = Math.Clamp(EndRatio * w, 0, w);

            // 2. Seçili olmayan sol ve sağ alanları karartma (dimming overlay)
            if (startX > 0)
            {
                dc.DrawRectangle(_dimBrush, null, new Rect(0, 0, startX, h));
            }

            if (endX < w)
            {
                dc.DrawRectangle(_dimBrush, null, new Rect(endX, 0, w - endX, h));
            }

            // 3. Sarı Seçim Çerçevesi (Üst ve Alt Hatlar)
            dc.DrawLine(_yellowFramePen, new Point(startX, 1.25), new Point(endX, 1.25));
            dc.DrawLine(_yellowFramePen, new Point(startX, h - 1.25), new Point(endX, h - 1.25));

            // 4. Sol Sarı Tutamaç (Handle)
            var handleWidth = 14.0;
            var leftHandleX = Math.Max(0, startX - handleWidth);
            var leftRect = new Rect(leftHandleX, 0, handleWidth, h);
            dc.DrawRoundedRectangle(_yellowHandleBrush, null, leftRect, 4, 4);

            // Sol tutamaç çentik çizgisi
            var leftNotchRect = new Rect(leftHandleX + (handleWidth / 2.0) - 1.0, centerY - 10, 2.0, 20);
            dc.DrawRoundedRectangle(_handleNotchBrush, null, leftNotchRect, 1, 1);

            // 5. Sağ Sarı Tutamaç (Handle)
            var rightHandleX = Math.Min(w - handleWidth, endX);
            var rightRect = new Rect(rightHandleX, 0, handleWidth, h);
            dc.DrawRoundedRectangle(_yellowHandleBrush, null, rightRect, 4, 4);

            // Sağ tutamaç çentik çizgisi
            var rightNotchRect = new Rect(rightHandleX + (handleWidth / 2.0) - 1.0, centerY - 10, 2.0, 20);
            dc.DrawRoundedRectangle(_handleNotchBrush, null, rightNotchRect, 1, 1);

            // 6. Playhead (Oynatma Konumu Çizgisi)
            if (PlayheadRatio.HasValue)
            {
                var playheadX = Math.Clamp(PlayheadRatio.Value * w, 0, w);
                if (playheadX >= startX && playheadX <= endX)
                {
                    dc.DrawLine(_playheadPen, new Point(playheadX, 0), new Point(playheadX, h));
                }
            }
        }
    }
}
