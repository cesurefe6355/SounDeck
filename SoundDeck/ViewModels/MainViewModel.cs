using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Threading;
using SoundDeck.Models;
using SoundDeck.Services;

namespace SoundDeck.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private const int HotkeyDebounceMilliseconds = 120;

    private readonly AudioDeviceService _deviceService;
    private readonly AudioEngine _audioEngine;
    private readonly ConfigService _configService;
    private readonly MicrophoneMixer _microphoneMixer;
    private readonly FolderScanner _folderScanner = new();
    private HotkeyService? _hotkeyService;
    private AudioDevice? _selectedMonitorDevice;
    private AudioDevice? _selectedBroadcastDevice;
    private AudioDevice? _selectedMicrophoneDevice;
    private string? _unresolvedMonitorDeviceId;
    private string? _unresolvedBroadcastDeviceId;
    private string? _unresolvedMicrophoneDeviceId;
    private bool _isMonitorEnabled = true;
    private bool _isBroadcastEnabled;
    private bool _isMicrophoneEnabled = true;
    private bool _isInitializing = true;
    private bool _isLoaded;
    private bool _isRefreshingDevices;
    private bool _isRefreshingMicrophones;
    private bool _hasVirtualCableCapture;
    private string? _microphoneStatusMessage;
    private readonly DispatcherTimer _microphoneListenWatchTimer;
    private string _statusMessage = "Hazır";
    private int _playingSoundCount;
    private string _searchText = string.Empty;
    private Guid _lastHotkeySoundId;
    private long _lastHotkeyAt;

    public MainViewModel(
        ConfigService configService,
        AudioDeviceService deviceService,
        AudioEngine audioEngine,
        MicrophoneMixer microphoneMixer)
    {
        _configService = configService;
        _deviceService = deviceService;
        _audioEngine = audioEngine;
        _microphoneMixer = microphoneMixer;

        SoundsView = CollectionViewSource.GetDefaultView(Sounds);
        SoundsView.Filter = FilterSound;
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);
        UseSuggestedBroadcastDeviceCommand = new RelayCommand(UseSuggestedBroadcastDevice);
        OpenMicrophoneSettingsCommand = new RelayCommand(OpenMicrophoneSettings);

        _microphoneListenWatchTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _microphoneListenWatchTimer.Tick += (_, _) => WatchWindowsListenSetting();
        _microphoneListenWatchTimer.Start();

        Sounds.CollectionChanged += (_, _) => NotifyFilterStateChanged();
        OutputDevices.CollectionChanged += (_, _) =>
        {
            if (!_isRefreshingDevices)
            {
                RaiseOutputRoutingChanged();
            }
        };

        _audioEngine.PlaybackStarted += OnPlaybackStarted;
        _audioEngine.PlaybackStopped += OnPlaybackStopped;
        _audioEngine.PlaybackFailed += OnPlaybackFailed;

        LoadConfiguration();
    }

    public event EventHandler<SoundItemViewModel>? EditSoundRequested;

    public event EventHandler<SoundItemViewModel>? RemoveSoundRequested;

    public ObservableCollection<SoundItemViewModel> Sounds { get; } = new();

    public ICollectionView SoundsView { get; }

    public ObservableCollection<AudioDevice> OutputDevices { get; } = new();

    public ObservableCollection<AudioDevice> CaptureDevices { get; } = new();

    public RelayCommand ClearSearchCommand { get; }

    public RelayCommand UseSuggestedBroadcastDeviceCommand { get; }

    public RelayCommand OpenMicrophoneSettingsCommand { get; }

    public string ConfigPath => _configService.ConfigPath;

    public int SoundCount => Sounds.Count;

    public bool HasSounds => Sounds.Count > 0;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (!SetProperty(ref _searchText, value ?? string.Empty))
            {
                return;
            }

            SoundsView.Refresh();
            NotifyFilterStateChanged();
        }
    }

    public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchText);

    public int FilteredSoundCount => SoundsView.Cast<object>().Count();

    public string CountDisplay => IsSearchActive && Sounds.Count > 0
        ? $"{FilteredSoundCount} / {Sounds.Count}"
        : Sounds.Count.ToString();

    public string EmptyStateTitle => IsSearchEmptyState
        ? "Eşleşen ses yok"
        : "Henüz ses eklenmedi";

    public string EmptyStateDescription => IsSearchEmptyState
        ? $"\"{SearchText.Trim()}\" aramasıyla eşleşen ses bulunamadı. Farklı bir arama deneyin veya aramayı temizleyin."
        : "Dosyalarınızı ekleyin veya bir klasörü tarayın. Eklediğiniz sesler otomatik olarak kaydedilir.";

    public string EmptyStateIcon => IsSearchEmptyState ? "\uE721" : "\uE8D6";

    public bool EmptyStateShowsAddActions => Sounds.Count == 0;

    public bool EmptyStateShowsClearSearch => IsSearchEmptyState;

    private bool IsSearchEmptyState => Sounds.Count > 0 && IsSearchActive;

    public AudioDevice? SelectedMonitorDevice
    {
        get => _selectedMonitorDevice;
        set
        {
            if (AreSameDevice(_selectedMonitorDevice, value))
            {
                return;
            }

            _selectedMonitorDevice = value;

            if (!_isRefreshingDevices && value is not null)
            {
                _unresolvedMonitorDeviceId = null;
            }

            OnPropertyChanged();
            RaiseOutputRoutingChanged();

            if (!_isInitializing && !_isRefreshingDevices)
            {
                SaveConfig();
            }
        }
    }

    public AudioDevice? SelectedBroadcastDevice
    {
        get => _selectedBroadcastDevice;
        set
        {
            if (AreSameDevice(_selectedBroadcastDevice, value))
            {
                return;
            }

            _selectedBroadcastDevice = value;

            if (!_isRefreshingDevices && value is not null)
            {
                _unresolvedBroadcastDeviceId = null;
            }

            OnPropertyChanged();
            RaiseOutputRoutingChanged();

            if (!_isInitializing && !_isRefreshingDevices)
            {
                SaveConfig();
            }

            if (!_isRefreshingDevices)
            {
                UpdateMicrophoneMixer();
            }
        }
    }

    public AudioDevice? SelectedMicrophoneDevice
    {
        get => _selectedMicrophoneDevice;
        set
        {
            if (AreSameDevice(_selectedMicrophoneDevice, value))
            {
                return;
            }

            _selectedMicrophoneDevice = value;

            if (!_isRefreshingMicrophones && value is not null)
            {
                _unresolvedMicrophoneDeviceId = null;
            }

            OnPropertyChanged();
            RaiseMicrophoneRoutingChanged();

            if (!_isInitializing && !_isRefreshingMicrophones)
            {
                SaveConfig();
                UpdateMicrophoneMixer();
            }
        }
    }

    public bool IsMicrophoneEnabled
    {
        get => _isMicrophoneEnabled;
        set
        {
            if (!SetProperty(ref _isMicrophoneEnabled, value))
            {
                return;
            }

            RaiseMicrophoneRoutingChanged();

            if (!_isInitializing && !_isRefreshingMicrophones)
            {
                SaveConfig();
                UpdateMicrophoneMixer();
            }
        }
    }

    public string MicrophoneRoutingMessage
    {
        get
        {
            if (_microphoneStatusMessage is not null)
            {
                return _microphoneStatusMessage;
            }

            if (CaptureDevices.Count == 0)
            {
                return "Mikrofon bulunamadı. Windows'ta bir mikrofon bağlayın.";
            }

            if (!IsMicrophoneEnabled)
            {
                return "Mikrofon karıştırma kapalı. Açmak için yandaki anahtarı kullanın.";
            }

            if (SelectedMicrophoneDevice is null)
            {
                return "Karıştırmak için bir mikrofon seçin.";
            }

            if (!IsBroadcastEnabled || SelectedBroadcastDevice is null)
            {
                return "Mikrofonu karıştırmak için B çıkışını açın.";
            }

            if (!IsVirtualMicrophoneDevice(SelectedBroadcastDevice))
            {
                return "Mikrofon karıştırma için B çıkışı CABLE Input gibi bir sanal mikrofon olmalı.";
            }

            return "Mikrofon + ses paneli B çıkışında birleştiriliyor.";
        }
    }

    public bool HasMicrophoneRoutingWarning =>
        _microphoneStatusMessage is not null ||
        (IsMicrophoneEnabled && CaptureDevices.Count == 0) ||
        (IsMicrophoneEnabled &&
         SelectedMicrophoneDevice is not null &&
         IsBroadcastEnabled &&
         SelectedBroadcastDevice is not null &&
         !IsVirtualMicrophoneDevice(SelectedBroadcastDevice));

    public bool IsMonitorEnabled
    {
        get => _isMonitorEnabled;
        set
        {
            if (!SetProperty(ref _isMonitorEnabled, value))
            {
                return;
            }

            if (value && SelectedMonitorDevice is null && OutputDevices.Count > 0)
            {
                StatusMessage = "Monitör çıkışı için bir cihaz seçin.";
            }

            RaiseOutputRoutingChanged();

            if (!_isInitializing && !_isRefreshingDevices)
            {
                SaveConfig();
            }
        }
    }

    public bool IsBroadcastEnabled
    {
        get => _isBroadcastEnabled;
        set
        {
            if (!SetProperty(ref _isBroadcastEnabled, value))
            {
                return;
            }

            if (value && SelectedBroadcastDevice is null && OutputDevices.Count > 0)
            {
                StatusMessage = "Yayın çıkışı için bir cihaz seçin.";
            }

            RaiseOutputRoutingChanged();

            if (!_isInitializing && !_isRefreshingDevices)
            {
                SaveConfig();
            }

            if (!_isRefreshingDevices)
            {
                UpdateMicrophoneMixer();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int PlayingSoundCount
    {
        get => _playingSoundCount;
        private set => SetProperty(ref _playingSoundCount, Math.Max(0, value));
    }

    public string? OutputRoutingWarning
    {
        get
        {
            if (SelectedMonitorDevice is not null &&
                SelectedBroadcastDevice is not null &&
                IsMonitorEnabled &&
                IsBroadcastEnabled &&
                AreSameDevice(SelectedMonitorDevice, SelectedBroadcastDevice))
            {
                return "B çıkışı A ile aynı. Karşı taraf sesi duymaz; B için CABLE girişi (VB-CABLE) veya Voicemeeter Input seçin.";
            }

            if (IsBroadcastEnabled && SelectedBroadcastDevice is null && OutputDevices.Count > 0)
            {
                return "Yayın çıkışı etkin ancak bir cihaz seçilmedi.";
            }

            return null;
        }
    }

    public bool HasOutputRoutingWarning => OutputRoutingWarning is not null;

    public bool HasVirtualDeviceSuggestion
    {
        get
        {
            var suggested = FindSuggestedBroadcastDevice();
            return suggested is not null && !AreSameDevice(SelectedBroadcastDevice, suggested);
        }
    }

    public bool HasNonSonarVirtualMicrophone =>
        OutputDevices.Any(device => IsVirtualMicrophoneDevice(device) && !IsSonarDevice(device));

    public bool HasVirtualCableDevice => OutputDevices.Any(IsVirtualCableDevice);

    public bool HasVirtualCableCapture => _hasVirtualCableCapture;

    public bool IsUsingSonarBroadcast => IsSonarDevice(SelectedBroadcastDevice);

    public bool NeedsVirtualMicSetup =>
        (!HasVirtualCableDevice && !HasNonSonarVirtualMicrophone) ||
        (IsVirtualCableDevice(SelectedBroadcastDevice) && !HasVirtualCableCapture);

    public string OutputRoutingMessage
    {
        get
        {
            if (OutputRoutingWarning is not null)
            {
                return OutputRoutingWarning;
            }

            if (!IsBroadcastEnabled)
            {
                return "Yayın çıkışı kapalı. Karşı tarafa ses göndermek için B çıkışını açın.";
            }

            if (SelectedBroadcastDevice is null)
            {
                return "Yayın çıkışı etkin ancak bir cihaz seçilmedi.";
            }

            if (IsUsingSonarBroadcast)
            {
                return HasVirtualCableDevice
                    ? "Sonar seçili. Daha hafif CABLE girişine geçmek için yandaki düğmeyi kullanın."
                    : "Sonar seçili. FPS düşüyorsa VB-CABLE'a geçin.";
            }

            if (IsVirtualCableDevice(SelectedBroadcastDevice))
            {
                return HasVirtualCableCapture
                    ? "B çıkışı CABLE girişi. Discord mikrofonunda CABLE Output seçin."
                    : "CABLE Output mikrofonu görünmüyor. Windows ses ayarlarını kontrol edin.";
            }

            if (!HasNonSonarVirtualMicrophone)
            {
                return "Sanal mikrofon için VB-CABLE kurun.";
            }

            return IsVirtualMicrophoneDevice(SelectedBroadcastDevice)
                ? "Yayın çıkışı sanal mikrofona bağlı. Discord/OBS mikrofonunda eşleşen sanal girişi seçin. Sanal mikrofonu ayrıca hoparlöre monitörlüyorsanız sesi iki kez duyabilirsiniz."
                : "Sesler etkin çıkışların tamamına aynı anda gönderilir.";
        }
    }

    public void Play(SoundItemViewModel sound)
    {
        if (!sound.IsAvailable)
        {
            sound.RefreshAvailability();

            if (!sound.IsAvailable)
            {
                StatusMessage = $"Dosya bulunamadı: {sound.FilePath}";
                return;
            }
        }

        var deviceIds = GetActiveDeviceIds();

        if (deviceIds.Count == 0)
        {
            StatusMessage = "Etkin bir çıkış cihazı seçili değil. Üst bölümden cihaz seçin.";
            return;
        }

        var started = _audioEngine.Play(sound.ToModel(), deviceIds);

        if (started > 0)
        {
            var message = started == deviceIds.Count
                ? $"'{sound.Name}' çalınıyor."
                : $"'{sound.Name}' kısmen çalınıyor. Bir çıkış başlatılamadı.";

            if (OutputRoutingWarning is not null)
            {
                message += " " + OutputRoutingWarning;
            }

            StatusMessage = message;
        }
    }

    public void PlayBySoundId(Guid soundId)
    {
        var sound = Sounds.FirstOrDefault(item => item.Id == soundId);

        if (sound is not null)
        {
            Play(sound);
        }
    }

    public void StopSound(SoundItemViewModel sound)
    {
        _audioEngine.StopSound(sound.Id);
        StatusMessage = $"'{sound.Name}' durduruldu.";
    }

    public void StopAll()
    {
        _audioEngine.StopAll();
        StatusMessage = "Tüm sesler durduruldu.";
    }

    public void AddFiles(IEnumerable<string> paths)
    {
        var files = new List<string>();

        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                files.AddRange(_folderScanner.Scan(path));
            }
            else
            {
                files.Add(path);
            }
        }

        if (files.Count == 0)
        {
            StatusMessage = "Desteklenen ses dosyası bulunamadı.";
            return;
        }

        ImportFiles(files, null);
    }

    public void AddFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            StatusMessage = "Seçilen klasör bulunamadı.";
            return;
        }

        var files = _folderScanner.Scan(folderPath);

        if (files.Count == 0)
        {
            StatusMessage = "Seçilen klasörde desteklenen ses dosyası bulunamadı.";
            return;
        }

        ImportFiles(files, $"{files.Count} dosya tarandı");
    }

    public bool UseVirtualCableDevice()
    {
        var device = OutputDevices.FirstOrDefault(IsVirtualCableDevice);

        if (device is null)
        {
            StatusMessage = "CABLE girişi bulunamadı. VB-CABLE kurulumundan sonra Cihazları Yenile'ye basın.";
            return false;
        }

        IsBroadcastEnabled = true;
        SelectedBroadcastDevice = device;

        StatusMessage = HasVirtualCableCapture
            ? "B çıkışı CABLE girişi olarak ayarlandı. Discord mikrofonunda CABLE Output seçin."
            : "B çıkışı CABLE girişi olarak ayarlandı ancak CABLE Output mikrofonu görünmüyor. Windows ses ayarlarını kontrol edin.";

        return true;
    }

    public void UseSuggestedBroadcastDevice()
    {
        if (HasVirtualCableDevice && UseVirtualCableDevice())
        {
            return;
        }

        var device = FindSuggestedBroadcastDevice();

        if (device is null)
        {
            StatusMessage = "Sanal mikrofon çıkış cihazı bulunamadı. Hafif çözüm için VB-CABLE kurun veya Voicemeeter kullanın.";
            return;
        }

        IsBroadcastEnabled = true;
        SelectedBroadcastDevice = device;
        SaveConfig();
        StatusMessage = $"Yayın çıkışı '{device.DisplayName}' olarak ayarlandı. Discord/OBS mikrofon ayarında eşleşen sanal girişi seçin.";
    }

    private static void OpenMicrophoneSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo("control.exe", "mmsys.cpl,,1")
            {
                UseShellExecute = true
            });
        }
        catch
        {
            // Windows ses denetim masası açılamazsa sessizce devam et.
        }
    }

    public void RefreshDevices()
    {
        var warning = RefreshDevicesCore(
            SelectedMonitorDevice?.Id ?? _unresolvedMonitorDeviceId,
            SelectedBroadcastDevice?.Id ?? _unresolvedBroadcastDeviceId);

        var microphoneWarning = RefreshCaptureDevices(
            SelectedMicrophoneDevice?.Id ?? _unresolvedMicrophoneDeviceId);

        SaveConfig();
        RaiseOutputRoutingChanged();
        UpdateMicrophoneMixer();

        if (warning is not null || microphoneWarning is not null)
        {
            StatusMessage = string.Join(
                " ",
                new[] { warning, microphoneWarning }.Where(message => !string.IsNullOrWhiteSpace(message)));
        }
        else if (OutputDevices.Count == 0)
        {
            StatusMessage = "Ses çıkış cihazı bulunamadı. Cihazı bağlayıp Yenile'ye basın.";
        }
        else
        {
            StatusMessage = $"{OutputDevices.Count} ses çıkış cihazı ve {CaptureDevices.Count} mikrofon bulundu.";
        }
    }

    public void RefreshAvailability()
    {
        var changed = 0;

        foreach (var sound in Sounds)
        {
            var wasAvailable = sound.IsAvailable;
            sound.RefreshAvailability();

            if (wasAvailable != sound.IsAvailable)
            {
                changed++;
            }
        }

        if (changed > 0)
        {
            var missing = Sounds.Count(sound => !sound.IsAvailable);
            StatusMessage = missing > 0
                ? $"{missing} ses dosyası bulunamadı. Dosyaları taşımış veya silmiş olabilirsiniz."
                : "Dosya yolları yeniden doğrulandı.";
        }
    }

    public void RemoveSound(SoundItemViewModel sound)
    {
        _hotkeyService?.Unregister(sound.Id);
        _audioEngine.StopSound(sound.Id);
        Sounds.Remove(sound);
        SaveConfig();
        StatusMessage = $"'{sound.Name}' listeden kaldırıldı.";
    }

    public string? ValidateHotkey(SoundItemViewModel current, HotkeyGesture? gesture)
    {
        if (gesture is null)
        {
            return null;
        }

        if (gesture.VirtualKey <= 0)
        {
            return "Geçerli bir tuş seçin.";
        }

        var conflict = Sounds.FirstOrDefault(sound =>
            sound.Id != current.Id &&
            sound.Hotkey is not null &&
            sound.Hotkey.Matches(gesture));

        return conflict is null
            ? null
            : $"Bu kısayol zaten '{conflict.Name}' sesine atanmış.";
    }

    public void ApplySoundEdits(
        SoundItemViewModel sound,
        string name,
        double volume,
        HotkeyGesture? hotkey,
        double startTimeSeconds = 0,
        double? endTimeSeconds = null)
    {
        sound.ApplyEdits(name, volume, hotkey, startTimeSeconds, endTimeSeconds);
        SoundsView.Refresh();
        NotifyFilterStateChanged();
        SaveConfig();
        StatusMessage = $"'{sound.Name}' güncellendi.";
    }

    public void AttachHotkeyService(HotkeyService hotkeyService)
    {
        if (_hotkeyService is not null)
        {
            _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
        }

        _hotkeyService = hotkeyService;
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
    }

    public void DetachHotkeyService()
    {
        if (_hotkeyService is null)
        {
            return;
        }

        _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
        _hotkeyService = null;
    }

    public IReadOnlyList<string> ReapplyHotkeys()
    {
        if (_hotkeyService is null)
        {
            return Array.Empty<string>();
        }

        _hotkeyService.UnregisterAll();
        var errors = new List<string>();
        var used = new Dictionary<(HotkeyModifiers Modifiers, int VirtualKey), string>();

        foreach (var sound in Sounds.Where(item => item.Hotkey is not null))
        {
            var hotkey = sound.Hotkey!;
            var key = (hotkey.Modifiers, hotkey.VirtualKey);

            if (used.TryGetValue(key, out var ownerName))
            {
                errors.Add($"{sound.Name}: kısayol zaten '{ownerName}' sesinde kullanılıyor.");
                continue;
            }

            var result = _hotkeyService.Register(sound.Id, hotkey);

            if (!result.IsSuccess)
            {
                errors.Add($"{sound.Name}: {result.Error}");
                continue;
            }

            used[key] = sound.Name;
        }

        if (errors.Count > 0)
        {
            StatusMessage = $"Kısayol uyarısı: {string.Join(" ", errors)}";
        }

        return errors;
    }

    public void MarkLoaded()
    {
        _isLoaded = true;
        UpdateMicrophoneMixer();
    }

    public void SaveConfig()
    {
        if (_isInitializing || !_isLoaded)
        {
            return;
        }

        var config = new AppConfig
        {
            Output = new OutputSettings
            {
                MonitorDeviceId = SelectedMonitorDevice?.Id ?? _unresolvedMonitorDeviceId,
                MonitorEnabled = IsMonitorEnabled,
                BroadcastDeviceId = SelectedBroadcastDevice?.Id ?? _unresolvedBroadcastDeviceId,
                BroadcastEnabled = IsBroadcastEnabled
            },
            Microphone = new MicrophoneSettings
            {
                DeviceId = SelectedMicrophoneDevice?.Id ?? _unresolvedMicrophoneDeviceId,
                Enabled = IsMicrophoneEnabled
            },
            Sounds = Sounds.Select(sound => sound.ToModel()).ToList()
        };

        try
        {
            _configService.Save(config);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ayarlar kaydedilemedi: {ex.Message}";
        }
    }

    internal void RequestEdit(SoundItemViewModel sound) =>
        EditSoundRequested?.Invoke(this, sound);

    internal void RequestRemove(SoundItemViewModel sound) =>
        RemoveSoundRequested?.Invoke(this, sound);

    private List<string> GetActiveDeviceIds()
    {
        var deviceIds = new List<string>(2);

        if (IsMonitorEnabled && SelectedMonitorDevice is not null)
        {
            deviceIds.Add(SelectedMonitorDevice.Id);
        }

        if (IsBroadcastEnabled && SelectedBroadcastDevice is not null)
        {
            deviceIds.Add(SelectedBroadcastDevice.Id);
        }

        return deviceIds
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void LoadConfiguration()
    {
        _isInitializing = true;

        try
        {
            var loadResult = _configService.Load();
            var config = loadResult.Config;
            var existingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var existingIds = new HashSet<Guid>();

            foreach (var sound in config.Sounds)
            {
                if (string.IsNullOrWhiteSpace(sound.FilePath) ||
                    !existingIds.Add(sound.Id) ||
                    !existingPaths.Add(sound.FilePath))
                {
                    continue;
                }

                Sounds.Add(new SoundItemViewModel(this, sound));
            }

            _isMonitorEnabled = config.Output.MonitorEnabled;
            _isBroadcastEnabled = config.Output.BroadcastEnabled;
            _isMicrophoneEnabled = config.Microphone.Enabled;
            OnPropertyChanged(nameof(IsMonitorEnabled));
            OnPropertyChanged(nameof(IsBroadcastEnabled));
            OnPropertyChanged(nameof(IsMicrophoneEnabled));

            var deviceWarning = RefreshDevicesCore(
                config.Output.MonitorDeviceId,
                config.Output.BroadcastDeviceId);

            var microphoneWarning = RefreshCaptureDevices(config.Microphone.DeviceId);

            _isInitializing = false;

            var messages = new List<string>();

            if (!string.IsNullOrWhiteSpace(loadResult.Warning))
            {
                messages.Add(loadResult.Warning!);
            }

            if (!string.IsNullOrWhiteSpace(deviceWarning))
            {
                messages.Add(deviceWarning!);
            }

            if (!string.IsNullOrWhiteSpace(microphoneWarning))
            {
                messages.Add(microphoneWarning!);
            }

            if (Sounds.Count > 0)
            {
                messages.Add($"{Sounds.Count} ses yüklendi.");
            }

            var missingFiles = Sounds.Count(sound => !sound.IsAvailable);

            if (missingFiles > 0)
            {
                messages.Add($"{missingFiles} dosya bulunamadı.");
            }

            if (OutputDevices.Count == 0)
            {
                messages.Add("Ses çıkış cihazı bulunamadı.");
            }

            if (messages.Count == 0)
            {
                messages.Add("Hazır. Dosya ekleyerek veya bir klasör tarayarak başlayın.");
            }

            StatusMessage = string.Join(" ", messages);
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private string? RefreshDevicesCore(string? preferredMonitorId, string? preferredBroadcastId)
    {
        var devices = _deviceService.GetRenderDevices();
        _hasVirtualCableCapture = _deviceService.IsCaptureDeviceAvailable(
            "CABLE Output",
            "CABLE-A Output",
            "CABLE-B Output");
        string? warning = null;

        _isRefreshingDevices = true;

        try
        {
            OutputDevices.Clear();

            foreach (var device in devices)
            {
                OutputDevices.Add(device);
            }

            var monitor = FindDevice(preferredMonitorId);

            if (monitor is null && string.IsNullOrWhiteSpace(preferredMonitorId))
            {
                var defaultId = _deviceService.GetDefaultRenderDeviceId();
                monitor = FindDevice(defaultId) ?? OutputDevices.FirstOrDefault();
            }

            if (monitor is null && !string.IsNullOrWhiteSpace(preferredMonitorId))
            {
                _unresolvedMonitorDeviceId = preferredMonitorId;
                warning = "Kayıtlı monitör çıkış cihazı bulunamadı.";
            }
            else
            {
                _unresolvedMonitorDeviceId = null;
            }

            var broadcast = FindDevice(preferredBroadcastId);

            if (broadcast is null && !string.IsNullOrWhiteSpace(preferredBroadcastId))
            {
                _unresolvedBroadcastDeviceId = preferredBroadcastId;
                warning = warning is null
                    ? "Kayıtlı yayın çıkış cihazı bulunamadı."
                    : warning + " Kayıtlı yayın çıkış cihazı bulunamadı.";
            }
            else
            {
                _unresolvedBroadcastDeviceId = null;
            }

            SelectedMonitorDevice = monitor;
            SelectedBroadcastDevice = broadcast;
        }
        finally
        {
            _isRefreshingDevices = false;
        }

        RaiseOutputRoutingChanged();
        return warning;
    }

    private string? RefreshCaptureDevices(string? preferredDeviceId)
    {
        var devices = _deviceService.GetCaptureDevices();
        string? warning = null;

        _isRefreshingMicrophones = true;

        try
        {
            CaptureDevices.Clear();

            foreach (var device in devices)
            {
                CaptureDevices.Add(device);
            }

            var microphone = FindCaptureDevice(preferredDeviceId);

            if (microphone is null && !string.IsNullOrWhiteSpace(preferredDeviceId))
            {
                _unresolvedMicrophoneDeviceId = preferredDeviceId;
                warning = "Kayıtlı mikrofon bulunamadı.";
            }
            else
            {
                _unresolvedMicrophoneDeviceId = null;

                if (microphone is null)
                {
                    var defaultId = _deviceService.GetDefaultCaptureDeviceId();
                    microphone = FindCaptureDevice(defaultId);
                }

                if (microphone is null)
                {
                    microphone = CaptureDevices.FirstOrDefault();
                }
            }

            SelectedMicrophoneDevice = microphone;
        }
        finally
        {
            _isRefreshingMicrophones = false;
        }

        RaiseMicrophoneRoutingChanged();
        return warning;
    }

    private void WatchWindowsListenSetting()
    {
        if (!_isLoaded ||
            !IsMicrophoneEnabled ||
            _microphoneStatusMessage is null ||
            !_microphoneStatusMessage.Contains("Dinle", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        UpdateMicrophoneMixer();
    }

    private void UpdateMicrophoneMixer()
    {
        if (!_isLoaded)
        {
            RaiseMicrophoneRoutingChanged();
            return;
        }

        if (!IsMicrophoneEnabled ||
            SelectedMicrophoneDevice is null ||
            SelectedBroadcastDevice is null ||
            !IsBroadcastEnabled ||
            !IsVirtualMicrophoneDevice(SelectedBroadcastDevice))
        {
            _microphoneMixer.Stop();
            _microphoneStatusMessage = null;
            RaiseMicrophoneRoutingChanged();
            return;
        }

        if (_deviceService.IsListenToDeviceEnabled(
                SelectedMicrophoneDevice.Id,
                SelectedBroadcastDevice.Id))
        {
            _microphoneMixer.Stop();
            _microphoneStatusMessage =
                "Windows 'Dinle' açık; çift sesi önlemek için karıştırma durduruldu. Dinle'yi kapatın; SoundDeck otomatik devralacak.";
            RaiseMicrophoneRoutingChanged();
            return;
        }

        if (_microphoneMixer.IsRunning &&
            string.Equals(
                _microphoneMixer.CaptureDeviceId,
                SelectedMicrophoneDevice.Id,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                _microphoneMixer.OutputDeviceId,
                SelectedBroadcastDevice.Id,
                StringComparison.OrdinalIgnoreCase))
        {
            _microphoneStatusMessage = null;
            RaiseMicrophoneRoutingChanged();
            return;
        }

        var started = _microphoneMixer.Start(
            SelectedMicrophoneDevice.Id,
            SelectedBroadcastDevice.Id);

        _microphoneStatusMessage = started
            ? null
            : $"Mikrofon karıştırma başlatılamadı: {_microphoneMixer.LastError ?? "Bilinmeyen hata."}";

        RaiseMicrophoneRoutingChanged();
    }

    private AudioDevice? FindDevice(string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return null;
        }

        return OutputDevices.FirstOrDefault(device =>
            string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase));
    }

    private AudioDevice? FindCaptureDevice(string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return null;
        }

        return CaptureDevices.FirstOrDefault(device =>
            string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsVirtualMicrophoneDevice(AudioDevice? device) =>
        device is not null &&
        device.DisplayName.Contains("sanal mikrofon", StringComparison.CurrentCultureIgnoreCase);

    private static bool IsVirtualCableDevice(AudioDevice? device) =>
        device is not null &&
        (device.DisplayName.Contains("CABLE Input", StringComparison.CurrentCultureIgnoreCase) ||
         device.DisplayName.Contains("CABLE-A Input", StringComparison.CurrentCultureIgnoreCase) ||
         device.DisplayName.Contains("CABLE-B Input", StringComparison.CurrentCultureIgnoreCase) ||
         device.DisplayName.Contains("VB-CABLE", StringComparison.CurrentCultureIgnoreCase));

    private static bool IsSonarDevice(AudioDevice? device) =>
        device is not null &&
        device.DisplayName.Contains("Sonar", StringComparison.CurrentCultureIgnoreCase);

    private AudioDevice? FindSuggestedBroadcastDevice()
    {
        // Sonar'ı otomatik önerme; hafif ve imzalı sanal sürücüleri tercih et.
        var candidates = OutputDevices
            .Where(device => IsVirtualMicrophoneDevice(device) && !IsSonarDevice(device))
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates.FirstOrDefault(IsVirtualCableDevice)
               ?? candidates.FirstOrDefault(device =>
                   device.DisplayName.Contains("Voicemeeter", StringComparison.CurrentCultureIgnoreCase))
               ?? candidates.FirstOrDefault(device =>
                   device.DisplayName.Contains("NVIDIA", StringComparison.CurrentCultureIgnoreCase))
               ?? candidates.FirstOrDefault(device => !AreSameDevice(device, SelectedMonitorDevice))
               ?? candidates[0];
    }

    private void ImportFiles(IReadOnlyCollection<string> files, string? sourceLabel)
    {
        var existingPaths = new HashSet<string>(
            Sounds.Select(sound => sound.FilePath),
            StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var duplicates = 0;
        var missing = 0;
        var unsupported = 0;
        var failed = 0;

        foreach (var file in files)
        {
            string fullPath;

            try
            {
                fullPath = Path.GetFullPath(file);

                if (!FolderScanner.IsSupported(fullPath))
                {
                    unsupported++;
                    continue;
                }

                if (!File.Exists(fullPath))
                {
                    missing++;
                    continue;
                }
            }
            catch
            {
                failed++;
                continue;
            }

            if (!existingPaths.Add(fullPath))
            {
                duplicates++;
                continue;
            }

            var sound = new SoundItem
            {
                Id = Guid.NewGuid(),
                Name = Path.GetFileNameWithoutExtension(fullPath),
                FilePath = fullPath,
                Volume = 0.85
            };

            if (string.IsNullOrWhiteSpace(sound.Name))
            {
                sound.Name = "İsimsiz ses";
            }

            Sounds.Add(new SoundItemViewModel(this, sound));
            added++;
        }

        if (added > 0)
        {
            SaveConfig();
        }

        var parts = new List<string>();

        if (sourceLabel is not null)
        {
            parts.Add(sourceLabel);
        }

        if (added > 0)
        {
            parts.Add($"{added} ses eklendi");
        }
        else if (duplicates > 0 && missing == 0 && unsupported == 0 && failed == 0)
        {
            parts.Add($"{duplicates} ses zaten listede");
            duplicates = 0;
        }
        else
        {
            parts.Add("Yeni ses eklenmedi");
        }

        if (duplicates > 0)
        {
            parts.Add($"{duplicates} zaten listede");
        }

        if (missing > 0)
        {
            parts.Add($"{missing} dosya bulunamadı");
        }

        if (unsupported > 0)
        {
            parts.Add($"{unsupported} desteklenmeyen dosya");
        }

        if (failed > 0)
        {
            parts.Add($"{failed} dosya okunamadı");
        }

        StatusMessage = string.Join(", ", parts) + ".";
    }

    private void OnPlaybackStarted(object? sender, PlaybackEventArgs e)
    {
        var sound = Sounds.FirstOrDefault(item => item.Id == e.SoundId);

        if (sound is null)
        {
            return;
        }

        var wasPlaying = sound.IsPlaying;
        sound.IncrementPlayback();

        if (!wasPlaying && sound.IsPlaying)
        {
            PlayingSoundCount++;
        }
    }

    private void OnPlaybackStopped(object? sender, PlaybackEventArgs e)
    {
        var sound = Sounds.FirstOrDefault(item => item.Id == e.SoundId);

        if (sound is null)
        {
            return;
        }

        var wasPlaying = sound.IsPlaying;
        sound.DecrementPlayback();

        if (wasPlaying && !sound.IsPlaying)
        {
            PlayingSoundCount--;
        }
    }

    private void OnPlaybackFailed(object? sender, string message)
    {
        StatusMessage = message;
    }

    private void OnHotkeyPressed(object? sender, Guid soundId)
    {
        // Aynı kısayoldan gelen yinelenen mesajları tek çalıma indir.
        var now = Environment.TickCount64;

        if (soundId == _lastHotkeySoundId &&
            now - _lastHotkeyAt < HotkeyDebounceMilliseconds)
        {
            return;
        }

        _lastHotkeySoundId = soundId;
        _lastHotkeyAt = now;
        PlayBySoundId(soundId);
    }

    private bool FilterSound(object item)
    {
        if (!IsSearchActive)
        {
            return true;
        }

        if (item is not SoundItemViewModel sound)
        {
            return false;
        }

        var query = SearchText.Trim();

        return sound.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
               sound.FileName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
               sound.FileType.Contains(query, StringComparison.CurrentCultureIgnoreCase);
    }

    private void NotifyFilterStateChanged()
    {
        OnPropertyChanged(nameof(SoundCount));
        OnPropertyChanged(nameof(HasSounds));
        OnPropertyChanged(nameof(FilteredSoundCount));
        OnPropertyChanged(nameof(CountDisplay));
        OnPropertyChanged(nameof(IsSearchActive));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateDescription));
        OnPropertyChanged(nameof(EmptyStateIcon));
        OnPropertyChanged(nameof(EmptyStateShowsAddActions));
        OnPropertyChanged(nameof(EmptyStateShowsClearSearch));
    }

    private static bool AreSameDevice(AudioDevice? left, AudioDevice? right)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }

        return string.Equals(left.Id, right.Id, StringComparison.OrdinalIgnoreCase);
    }

    private void RaiseOutputRoutingChanged()
    {
        OnPropertyChanged(nameof(OutputRoutingWarning));
        OnPropertyChanged(nameof(HasOutputRoutingWarning));
        OnPropertyChanged(nameof(OutputRoutingMessage));
        OnPropertyChanged(nameof(HasVirtualDeviceSuggestion));
        OnPropertyChanged(nameof(HasNonSonarVirtualMicrophone));
        OnPropertyChanged(nameof(HasVirtualCableDevice));
        OnPropertyChanged(nameof(HasVirtualCableCapture));
        OnPropertyChanged(nameof(IsUsingSonarBroadcast));
        OnPropertyChanged(nameof(NeedsVirtualMicSetup));
    }

    private void RaiseMicrophoneRoutingChanged()
    {
        OnPropertyChanged(nameof(MicrophoneRoutingMessage));
        OnPropertyChanged(nameof(HasMicrophoneRoutingWarning));
    }
}
