using System.Windows.Input;
using SoundDeck.Models;

namespace SoundDeck.ViewModels;

public sealed class SoundItemViewModel : ViewModelBase
{
    private readonly MainViewModel _owner;
    private string _name;
    private double _volume;
    private HotkeyGesture? _hotkey;
    private double _startTimeSeconds;
    private double? _endTimeSeconds;
    private bool _isAvailable;
    private int _activePlaybackCount;

    public SoundItemViewModel(MainViewModel owner, SoundItem model)
    {
        _owner = owner;
        Id = model.Id;
        FilePath = model.FilePath;
        _name = model.Name;
        _volume = Math.Clamp(model.Volume, 0, 1);
        _hotkey = model.Hotkey?.Clone();
        _startTimeSeconds = Math.Max(0, model.StartTimeSeconds);
        _endTimeSeconds = model.EndTimeSeconds.HasValue ? Math.Max(0, model.EndTimeSeconds.Value) : null;
        _isAvailable = File.Exists(model.FilePath);

        PlayCommand = new RelayCommand(Play, () => IsAvailable);
        StopCommand = new RelayCommand(Stop, () => IsPlaying);
        EditCommand = new RelayCommand(() => _owner.RequestEdit(this));
        RemoveCommand = new RelayCommand(() => _owner.RequestRemove(this));
    }

    public Guid Id { get; }

    public string FilePath { get; }

    public string FileName => Path.GetFileName(FilePath);

    public string FileType
    {
        get
        {
            var extension = Path.GetExtension(FilePath).TrimStart('.');
            return string.IsNullOrWhiteSpace(extension) ? "SES" : extension.ToUpperInvariant();
        }
    }

    public string Name
    {
        get => _name;
        private set => SetProperty(ref _name, value);
    }

    public double Volume
    {
        get => _volume;
        private set
        {
            var clamped = Math.Clamp(value, 0, 1);

            if (SetProperty(ref _volume, clamped))
            {
                OnPropertyChanged(nameof(VolumeDisplay));
            }
        }
    }

    public string VolumeDisplay => $"{Volume * 100:0}%";

    public HotkeyGesture? Hotkey => _hotkey;

    public string HotkeyDisplay => _hotkey is null ? "Kısayol yok" : _hotkey.DisplayText;

    public string HotkeyBadgeText => _hotkey is null ? "Kısayol ata" : _hotkey.DisplayText;

    public string HotkeyToolTip => _hotkey is null
        ? "Bu ses için tek tuş veya kombinasyon ata"
        : _hotkey.Modifiers == HotkeyModifiers.None
            ? $"{_hotkey.DisplayText} — tek tuş, sistem genelinde çalışır"
            : $"{_hotkey.DisplayText} — düzenlemek için tıkla";

    public bool HasHotkey => _hotkey is not null;

    public bool IsSingleKeyHotkey => _hotkey is { Modifiers: HotkeyModifiers.None };

    public double StartTimeSeconds
    {
        get => _startTimeSeconds;
        private set
        {
            if (SetProperty(ref _startTimeSeconds, Math.Max(0, value)))
            {
                OnPropertyChanged(nameof(HasTrim));
                OnPropertyChanged(nameof(TrimDisplay));
                OnPropertyChanged(nameof(TrimToolTip));
            }
        }
    }

    public double? EndTimeSeconds
    {
        get => _endTimeSeconds;
        private set
        {
            var normalized = value.HasValue ? Math.Max(0, value.Value) : (double?)null;
            if (SetProperty(ref _endTimeSeconds, normalized))
            {
                OnPropertyChanged(nameof(HasTrim));
                OnPropertyChanged(nameof(TrimDisplay));
                OnPropertyChanged(nameof(TrimToolTip));
            }
        }
    }

    public bool HasTrim => StartTimeSeconds > 0.05 || (EndTimeSeconds.HasValue && EndTimeSeconds.Value > 0);

    public string TrimDisplay
    {
        get
        {
            if (!HasTrim)
            {
                return string.Empty;
            }

            var start = FormatTime(StartTimeSeconds);
            var end = EndTimeSeconds.HasValue ? FormatTime(EndTimeSeconds.Value) : "Son";
            return $"{start} - {end}";
        }
    }

    public string TrimToolTip
    {
        get
        {
            if (!HasTrim)
            {
                return string.Empty;
            }

            var start = FormatTime(StartTimeSeconds);
            var end = EndTimeSeconds.HasValue ? FormatTime(EndTimeSeconds.Value) : "Dosya sonu";
            return $"Kırpılmış aralık: {start} → {end}";
        }
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss\.f")
            : ts.ToString(@"m\:ss\.f");
    }

    public bool IsAvailable
    {
        get => _isAvailable;
        private set
        {
            if (SetProperty(ref _isAvailable, value))
            {
                OnPropertyChanged(nameof(AvailabilityText));
                PlayCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string AvailabilityText => IsAvailable ? string.Empty : "Dosya bulunamadı";

    public int ActivePlaybackCount => _activePlaybackCount;

    public bool IsPlaying => _activePlaybackCount > 0;

    public string PlayingLabel => IsPlaying ? "Çalıyor" : string.Empty;

    public RelayCommand PlayCommand { get; }

    public RelayCommand StopCommand { get; }

    public RelayCommand EditCommand { get; }

    public RelayCommand RemoveCommand { get; }

    public void IncrementPlayback() => SetPlaybackCount(_activePlaybackCount + 1);

    public void DecrementPlayback() => SetPlaybackCount(_activePlaybackCount - 1);

    public void ResetPlayback() => SetPlaybackCount(0);

    public void RefreshAvailability()
    {
        IsAvailable = File.Exists(FilePath);
    }

    public void ApplyEdits(string name, double volume, HotkeyGesture? hotkey, double startTimeSeconds = 0, double? endTimeSeconds = null)
    {
        Name = name.Trim();
        Volume = volume;
        _hotkey = hotkey?.Clone();
        StartTimeSeconds = startTimeSeconds;
        EndTimeSeconds = endTimeSeconds;

        OnPropertyChanged(nameof(Hotkey));
        OnPropertyChanged(nameof(HotkeyDisplay));
        OnPropertyChanged(nameof(HotkeyBadgeText));
        OnPropertyChanged(nameof(HotkeyToolTip));
        OnPropertyChanged(nameof(HasHotkey));
        OnPropertyChanged(nameof(IsSingleKeyHotkey));
        OnPropertyChanged(nameof(HasTrim));
        OnPropertyChanged(nameof(TrimDisplay));
        OnPropertyChanged(nameof(TrimToolTip));
    }

    public SoundItem ToModel() => new()
    {
        Id = Id,
        Name = Name,
        FilePath = FilePath,
        Volume = Volume,
        Hotkey = _hotkey?.Clone(),
        StartTimeSeconds = StartTimeSeconds,
        EndTimeSeconds = EndTimeSeconds
    };

    private void Play() => _owner.Play(this);

    private void Stop() => _owner.StopSound(this);

    private void SetPlaybackCount(int count)
    {
        var clamped = Math.Max(0, count);

        if (_activePlaybackCount == clamped)
        {
            return;
        }

        _activePlaybackCount = clamped;
        OnPropertyChanged(nameof(ActivePlaybackCount));
        OnPropertyChanged(nameof(IsPlaying));
        OnPropertyChanged(nameof(PlayingLabel));
        StopCommand.RaiseCanExecuteChanged();
    }
}
