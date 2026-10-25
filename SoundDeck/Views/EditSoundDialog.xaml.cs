using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundDeck.Models;
using SoundDeck.Services;
using SoundDeck.ViewModels;

namespace SoundDeck.Views;

public partial class EditSoundDialog : Window
{
    private readonly SoundItemViewModel _sound;
    private readonly double _totalDurationSeconds;
    private HotkeyGesture? _hotkey;
    private bool _isUpdatingUi;

    private WasapiOut? _previewOut;
    private WaveStream? _previewReader;
    private DispatcherTimer? _previewTimer;
    private DateTime _previewStartedAt;
    private double _previewStartOffset;
    private double _previewDuration;

    public EditSoundDialog(
        SoundItemViewModel sound,
        string initialName,
        double initialVolume,
        HotkeyGesture? initialHotkey,
        double initialStartTime,
        double? initialEndTime,
        bool focusHotkey = false)
    {
        InitializeComponent();

        _sound = sound;
        _hotkey = initialHotkey?.Clone();

        FileNameText.Text = sound.FileName;
        FilePathText.Text = sound.FilePath;
        FileTypeText.Text = sound.FileType;
        NameBox.Text = initialName;
        VolumeSlider.Value = Math.Clamp(initialVolume, 0, 1) * 100;

        var duration = AudioEditorService.GetDuration(sound.FilePath);
        _totalDurationSeconds = duration.TotalSeconds > 0 ? duration.TotalSeconds : 0;
        DurationText.Text = _totalDurationSeconds > 0 ? FormatTime(_totalDurationSeconds) : string.Empty;

        // Waveform trimmer kontrolünü başlat
        WaveformTrimmer.LoadAudio(sound.FilePath, initialStartTime, initialEndTime);
        WaveformTrimmer.RangeChanged += OnWaveformRangeChanged;
        WaveformTrimmer.PlayToggleRequested += (_, _) => TogglePreview();
        WaveformTrimmer.SeekRequested += (_, seekSec) => SeekPreview(seekSec);

        UpdateInputBoxes(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
        UpdateTrimSummary(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
        UpdateHotkeyVisual();

        SourceInitialized += (_, _) => WindowChromeHelper.ApplyDarkTitleBar(this);
        Closing += (_, _) => StopPreview();

        Loaded += (_, _) =>
        {
            if (focusHotkey)
            {
                HotkeyBox.Focus();
                return;
            }

            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    public EditSoundDialog(
        SoundItemViewModel sound,
        string initialName,
        double initialVolume,
        HotkeyGesture? initialHotkey,
        bool focusHotkey = false)
        : this(sound, initialName, initialVolume, initialHotkey, sound.StartTimeSeconds, sound.EndTimeSeconds, focusHotkey)
    {
    }

    public string SoundName { get; private set; } = string.Empty;

    public double Volume { get; private set; }

    public HotkeyGesture? Hotkey => _hotkey?.Clone();

    public double StartTimeSeconds { get; private set; }

    public double? EndTimeSeconds { get; private set; }

    public string? NewlyExportedFilePath { get; private set; }

    private void OnWaveformRangeChanged(object? sender, (double Start, double End) range)
    {
        UpdateInputBoxes(range.Start, range.End);
        UpdateTrimSummary(range.Start, range.End);
    }

    private void UpdateInputBoxes(double start, double end)
    {
        _isUpdatingUi = true;
        StartTimeBox.Text = FormatTime(start);
        EndTimeBox.Text = FormatTime(end);
        _isUpdatingUi = false;
    }

    private void StartTimeBox_LostFocus(object sender, RoutedEventArgs e) => CommitStartTimeBox();

    private void StartTimeBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitStartTimeBox();
            e.Handled = true;
        }
    }

    private void CommitStartTimeBox()
    {
        if (_isUpdatingUi || _totalDurationSeconds <= 0) return;

        if (TryParseTime(StartTimeBox.Text, out var seconds))
        {
            var clamped = Math.Clamp(seconds, 0, _totalDurationSeconds);
            WaveformTrimmer.StartSeconds = clamped;
            UpdateInputBoxes(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
            UpdateTrimSummary(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
        }
        else
        {
            StartTimeBox.Text = FormatTime(WaveformTrimmer.StartSeconds);
        }
    }

    private void EndTimeBox_LostFocus(object sender, RoutedEventArgs e) => CommitEndTimeBox();

    private void EndTimeBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitEndTimeBox();
            e.Handled = true;
        }
    }

    private void CommitEndTimeBox()
    {
        if (_isUpdatingUi || _totalDurationSeconds <= 0) return;

        if (TryParseTime(EndTimeBox.Text, out var seconds))
        {
            var clamped = Math.Clamp(seconds, 0, _totalDurationSeconds);
            WaveformTrimmer.EndSeconds = clamped;
            UpdateInputBoxes(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
            UpdateTrimSummary(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
        }
        else
        {
            EndTimeBox.Text = FormatTime(WaveformTrimmer.EndSeconds);
        }
    }

    private void SetStartZero_Click(object sender, RoutedEventArgs e)
    {
        if (_totalDurationSeconds <= 0) return;
        WaveformTrimmer.StartSeconds = 0;
        UpdateInputBoxes(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
        UpdateTrimSummary(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
    }

    private void SetEndMax_Click(object sender, RoutedEventArgs e)
    {
        if (_totalDurationSeconds <= 0) return;
        WaveformTrimmer.EndSeconds = _totalDurationSeconds;
        UpdateInputBoxes(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
        UpdateTrimSummary(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
    }

    private void ResetTrim_Click(object sender, RoutedEventArgs e)
    {
        StopPreview();
        WaveformTrimmer.ResetTrim();
        UpdateInputBoxes(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
        UpdateTrimSummary(WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
    }

    private void UpdateTrimSummary(double start, double end)
    {
        if (_totalDurationSeconds <= 0)
        {
            TrimSummaryText.Text = "Süre bilgisi alınamadı";
            return;
        }

        var diff = Math.Max(0, end - start);
        var isFull = start <= 0.05 && end >= (_totalDurationSeconds - 0.05);

        TrimSummaryText.Text = isFull
            ? $"{FormatTime(start)} - {FormatTime(end)} (Tamamı: {diff:0.2} sn)"
            : $"{FormatTime(start)} - {FormatTime(end)} (Seçili: {diff:0.2} sn / Toplam: {_totalDurationSeconds:0.2} sn)";
    }

    private void TogglePreview()
    {
        if (_previewOut is not null)
        {
            StopPreview();
            return;
        }

        StartPreview(WaveformTrimmer.StartSeconds);
    }

    private void SeekPreview(double seekTime)
    {
        if (_previewOut is not null)
        {
            StopPreview();
            StartPreview(seekTime);
        }
        else
        {
            WaveformTrimmer.SetPlaybackPosition(seekTime);
        }
    }

    private void StartPreview(double fromTime)
    {
        StopPreview();

        if (!File.Exists(_sound.FilePath))
        {
            ShowError("Ses dosyası bulunamadı.");
            return;
        }

        var start = Math.Clamp(fromTime, WaveformTrimmer.StartSeconds, WaveformTrimmer.EndSeconds);
        var end = WaveformTrimmer.EndSeconds;
        var duration = end - start;

        if (duration <= 0.05)
        {
            ShowError("Önizleme için başlangıç ve bitiş arasında en az 0.1 saniye olmalıdır.");
            return;
        }

        try
        {
            _previewReader = AudioEditorService.CreateReader(_sound.FilePath);

            if (start > 0.02 && start < _previewReader.TotalTime.TotalSeconds)
            {
                try
                {
                    _previewReader.CurrentTime = TimeSpan.FromSeconds(start);
                }
                catch
                {
                }
            }

            var sampleProvider = _previewReader is ISampleProvider sp ? sp : _previewReader.ToSampleProvider();
            sampleProvider = new TrimmedSampleProvider(sampleProvider, TimeSpan.FromSeconds(duration));

            var volumeProvider = new VolumeSampleProvider(sampleProvider)
            {
                Volume = (float)Math.Clamp(VolumeSlider.Value / 100.0, 0, 1)
            };

            _previewOut = new WasapiOut(AudioClientShareMode.Shared, 80);
            _previewOut.PlaybackStopped += (_, _) => Dispatcher.Invoke(StopPreview);
            _previewOut.Init(volumeProvider);
            _previewOut.Play();

            _previewStartedAt = DateTime.UtcNow;
            _previewStartOffset = start;
            _previewDuration = duration;

            _previewTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            _previewTimer.Tick += PreviewTimer_Tick;
            _previewTimer.Start();

            WaveformTrimmer.SetPlayingState(true);
            ShowError(null);
        }
        catch (Exception ex)
        {
            StopPreview();
            ShowError($"Önizleme başlatılamadı: {ex.Message}");
        }
    }

    private void PreviewTimer_Tick(object? sender, EventArgs e)
    {
        if (_previewOut is null) return;

        var elapsed = (DateTime.UtcNow - _previewStartedAt).TotalSeconds;
        var current = _previewStartOffset + elapsed;

        if (current >= _previewStartOffset + _previewDuration)
        {
            StopPreview();
            return;
        }

        WaveformTrimmer.SetPlaybackPosition(current);
    }

    private void StopPreview()
    {
        if (_previewTimer is not null)
        {
            _previewTimer.Stop();
            _previewTimer.Tick -= PreviewTimer_Tick;
            _previewTimer = null;
        }

        if (_previewOut is not null)
        {
            try
            {
                _previewOut.Stop();
            }
            catch { }

            try
            {
                _previewOut.Dispose();
            }
            catch { }

            _previewOut = null;
        }

        if (_previewReader is not null)
        {
            try
            {
                _previewReader.Dispose();
            }
            catch { }

            _previewReader = null;
        }

        WaveformTrimmer.SetPlayingState(false);
        WaveformTrimmer.SetPlaybackPosition(null);
    }

    private void TrimAndExport_Click(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(_sound.FilePath))
        {
            ShowError("Ses dosyası bulunamadı.");
            return;
        }

        var start = WaveformTrimmer.StartSeconds;
        var end = WaveformTrimmer.EndSeconds;

        if (end <= start)
        {
            ShowError("Bitiş süresi başlangıç süresinden büyük olmalıdır.");
            return;
        }

        var initialDirectory = Path.GetDirectoryName(_sound.FilePath);
        var baseName = Path.GetFileNameWithoutExtension(_sound.FilePath);

        var saveDialog = new SaveFileDialog
        {
            Title = "Kırpılan Sesi Farklı Kaydet",
            Filter = "WAV Dosyası (*.wav)|*.wav",
            InitialDirectory = initialDirectory,
            FileName = $"{baseName}_kesilmis.wav"
        };

        if (saveDialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            AudioEditorService.TrimToWav(
                _sound.FilePath,
                saveDialog.FileName,
                TimeSpan.FromSeconds(start),
                TimeSpan.FromSeconds(end));

            var result = MessageBox.Show(
                this,
                $"Ses başarıyla kesildi ve kaydedildi:\n\n{saveDialog.FileName}\n\nBu yeni kesilen sesi SoundDeck listenize eklemek ister misiniz?",
                "Ses Kesildi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                NewlyExportedFilePath = saveDialog.FileName;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Ses kesilirken hata oluştu: {ex.Message}");
        }
    }

    private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var modifiers = Keyboard.Modifiers;

        if (key == Key.None || IsModifierKey(key))
        {
            e.Handled = true;
            return;
        }

        if (key == Key.Tab && (modifiers == ModifierKeys.None || modifiers == ModifierKeys.Shift))
        {
            return;
        }

        if (key == Key.Escape && modifiers == ModifierKeys.None)
        {
            e.Handled = true;
            DialogResult = false;
            return;
        }

        e.Handled = true;

        if (modifiers == ModifierKeys.None && !IsSafeSingleKey(key))
        {
            ShowError("Boşluk, Enter, yön tuşları gibi sistem tuşları tek başına atanamaz. Bunlar için Ctrl, Alt, Shift veya Win ile birlikte kullanın.");
            return;
        }

        var virtualKey = KeyInterop.VirtualKeyFromKey(key);

        if (virtualKey <= 0)
        {
            ShowError("Bu tuş bir kısayol olarak kullanılamıyor.");
            return;
        }

        _hotkey = new HotkeyGesture
        {
            Modifiers = ToHotkeyModifiers(modifiers),
            VirtualKey = virtualKey
        };

        ShowError(null);
        UpdateHotkeyVisual();
    }

    private void ClearHotkey_Click(object sender, RoutedEventArgs e)
    {
        _hotkey = null;
        ShowError(null);
        UpdateHotkeyVisual();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("Ses adı boş olamaz.");
            NameBox.Focus();
            return;
        }

        if (name.Length > 80)
        {
            ShowError("Ses adı en fazla 80 karakter olabilir.");
            NameBox.Focus();
            return;
        }

        var start = WaveformTrimmer.StartSeconds;
        var end = WaveformTrimmer.EndSeconds;

        if (_totalDurationSeconds > 0)
        {
            if (end <= start)
            {
                ShowError("Bitiş zamanı başlangıç zamanından büyük olmalıdır.");
                return;
            }

            StartTimeSeconds = start > 0.05 ? start : 0;
            EndTimeSeconds = end < (_totalDurationSeconds - 0.05) ? end : null;
        }
        else
        {
            StartTimeSeconds = 0;
            EndTimeSeconds = null;
        }

        StopPreview();

        SoundName = name;
        Volume = Math.Clamp(VolumeSlider.Value / 100.0, 0, 1);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        StopPreview();
        DialogResult = false;
    }

    private void UpdateHotkeyVisual()
    {
        HotkeyBox.Text = _hotkey?.DisplayText ?? "Kısayol atamak için tıklayın";
        HotkeyBox.Foreground = (Brush)FindResource(
            _hotkey is null ? "TextMutedBrush" : "TextPrimaryBrush");
        SingleKeyWarning.Visibility = _hotkey is { Modifiers: HotkeyModifiers.None }
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ShowError(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            ErrorText.Text = string.Empty;
            ErrorText.Visibility = Visibility.Collapsed;
            return;
        }

        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss\.ff", CultureInfo.InvariantCulture)
            : ts.ToString(@"mm\:ss\.ff", CultureInfo.InvariantCulture);
    }

    private static bool TryParseTime(string text, out double seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        text = text.Trim().Replace(',', '.');

        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var directSec))
        {
            seconds = directSec;
            return true;
        }

        var parts = text.Split(':');
        if (parts.Length == 2)
        {
            if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var m) &&
                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var s))
            {
                seconds = m * 60 + s;
                return true;
            }
        }
        else if (parts.Length == 3)
        {
            if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var h) &&
                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var m) &&
                double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var s))
            {
                seconds = h * 3600 + m * 60 + s;
                return true;
            }
        }

        return false;
    }

    private static bool IsModifierKey(Key key) =>
        key is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift
            or Key.LWin or Key.RWin;

    private static bool IsSafeSingleKey(Key key)
    {
        if (key is >= Key.A and <= Key.Z) return true;
        if (key is >= Key.D0 and <= Key.D9) return true;
        if (key is >= Key.NumPad0 and <= Key.NumPad9) return true;
        if (key is >= Key.F1 and <= Key.F24) return true;

        return key is Key.OemTilde
            or Key.OemMinus
            or Key.OemPlus
            or Key.OemOpenBrackets
            or Key.OemCloseBrackets
            or Key.OemPipe
            or Key.OemSemicolon
            or Key.OemQuotes
            or Key.OemComma
            or Key.OemPeriod
            or Key.OemQuestion
            or Key.Multiply
            or Key.Add
            or Key.Subtract
            or Key.Divide;
    }

    private static HotkeyModifiers ToHotkeyModifiers(ModifierKeys modifiers)
    {
        var result = HotkeyModifiers.None;

        if ((modifiers & ModifierKeys.Control) != 0)
        {
            result |= HotkeyModifiers.Control;
        }

        if ((modifiers & ModifierKeys.Alt) != 0)
        {
            result |= HotkeyModifiers.Alt;
        }

        if ((modifiers & ModifierKeys.Shift) != 0)
        {
            result |= HotkeyModifiers.Shift;
        }

        if ((modifiers & ModifierKeys.Windows) != 0)
        {
            result |= HotkeyModifiers.Win;
        }

        return result;
    }
}
