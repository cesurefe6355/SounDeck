using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using SoundDeck.Models;
using SoundDeck.Services;
using SoundDeck.ViewModels;
using SoundDeck.Views;

namespace SoundDeck;

public partial class MainWindow : Window
{
    private readonly AudioDeviceService _deviceService;
    private readonly AudioEngine _audioEngine;
    private readonly MicrophoneMixer _microphoneMixer;
    private readonly ConfigService _configService;
    private readonly MainViewModel _viewModel;
    private readonly HotkeyService _hotkeyService = new();
    private bool _hotkeysInitialized;
    private bool _isClosing;

    public MainWindow()
    {
        InitializeComponent();

        _configService = new ConfigService();
        _deviceService = new AudioDeviceService();
        _audioEngine = new AudioEngine(_deviceService);
        _microphoneMixer = new MicrophoneMixer(_deviceService);
        _viewModel = new MainViewModel(_configService, _deviceService, _audioEngine, _microphoneMixer);

        _viewModel.EditSoundRequested += OnEditSoundRequested;
        _viewModel.RemoveSoundRequested += OnRemoveSoundRequested;

        DataContext = _viewModel;

        SourceInitialized += OnSourceInitialized;
        Loaded += (_, _) => _viewModel.MarkLoaded();
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        WindowChromeHelper.ApplyDarkTitleBar(this);

        try
        {
            _hotkeyService.Initialize(this);
            _viewModel.AttachHotkeyService(_hotkeyService);
            _viewModel.ReapplyHotkeys();
            _hotkeysInitialized = true;
        }
        catch (Exception ex)
        {
            _viewModel.StatusMessage = $"Global kısayollar başlatılamadı: {ex.Message}";
        }
    }

    private void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Ses dosyaları seç",
            Filter = "Desteklenen ses dosyaları (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|Tüm dosyalar (*.*)|*.*",
            Multiselect = true,
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.AddFiles(dialog.FileNames);
        }
    }

    private void ScanFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Ses dosyalarının bulunduğu klasörü seç",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.AddFromFolder(dialog.FolderName);
        }
    }

    private void StopAll_Click(object sender, RoutedEventArgs e) => _viewModel.StopAll();

    private void RefreshDevices_Click(object sender, RoutedEventArgs e) => _viewModel.RefreshDevices();

    private void OpenVirtualMicSetup_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VirtualMicSetupDialog(_viewModel)
        {
            Owner = this
        };

        dialog.ShowDialog();
    }

    private void SoundCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (IsInsideButton(e.OriginalSource) || sender is not FrameworkElement { DataContext: SoundItemViewModel sound })
        {
            return;
        }

        _viewModel.Play(sound);
    }

    private void PlaySoundButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is { } sound)
        {
            _viewModel.Play(sound);
        }
    }

    private void StopSoundButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is { } sound)
        {
            _viewModel.StopSound(sound);
        }
    }

    private void EditSoundButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is { } sound)
        {
            EditSound(sound);
        }
    }

    private void EditHotkeyButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is { } sound)
        {
            EditSound(sound, focusHotkey: true);
        }
    }

    private void EditHotkeyMenu_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is { } sound)
        {
            EditSound(sound, focusHotkey: true);
        }
    }

    private void PlaySoundMenu_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is { } sound)
        {
            _viewModel.Play(sound);
        }
    }

    private void StopSoundMenu_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is { } sound)
        {
            _viewModel.StopSound(sound);
        }
    }

    private void EditSoundMenu_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is { } sound)
        {
            EditSound(sound);
        }
    }

    private void SoundContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu { PlacementTarget: FrameworkElement target } menu)
        {
            menu.DataContext = target.DataContext;
        }
    }

    private void RemoveSoundMenu_Click(object sender, RoutedEventArgs e)
    {
        if (GetSoundFromMenuItem(sender) is not { } sound)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            $"'{sound.Name}' listeden kaldırılsın mı?\n\nDosya diskten silinmez.",
            "Sesi Kaldır",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.RemoveSound(sound);
        }
    }

    private void EditSound(SoundItemViewModel sound, bool focusHotkey = false)
    {
        // Düzenleme penceresinde metin yazarken global kısayolların araya girmesini önle.
        if (_hotkeysInitialized)
        {
            _hotkeyService.UnregisterAll();
        }

        var initialName = sound.Name;
        var initialVolume = sound.Volume;
        var initialHotkey = sound.Hotkey;
        var initialStartTime = sound.StartTimeSeconds;
        var initialEndTime = sound.EndTimeSeconds;
        var applied = false;
        IReadOnlyList<string> hotkeyErrors = Array.Empty<string>();

        try
        {
            while (true)
            {
                var dialog = new EditSoundDialog(
                    sound,
                    initialName,
                    initialVolume,
                    initialHotkey,
                    initialStartTime,
                    initialEndTime,
                    focusHotkey)
                {
                    Owner = this
                };

                if (dialog.ShowDialog() != true)
                {
                    if (!string.IsNullOrWhiteSpace(dialog.NewlyExportedFilePath))
                    {
                        _viewModel.AddFiles(new[] { dialog.NewlyExportedFilePath });
                    }
                    break;
                }

                var validationError = _viewModel.ValidateHotkey(sound, dialog.Hotkey);

                if (validationError is not null)
                {
                    MessageBox.Show(
                        this,
                        validationError,
                        "Kısayol Çakışması",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    initialName = dialog.SoundName;
                    initialVolume = dialog.Volume;
                    initialHotkey = dialog.Hotkey;
                    initialStartTime = dialog.StartTimeSeconds;
                    initialEndTime = dialog.EndTimeSeconds;
                    continue;
                }

                _viewModel.ApplySoundEdits(
                    sound,
                    dialog.SoundName,
                    dialog.Volume,
                    dialog.Hotkey,
                    dialog.StartTimeSeconds,
                    dialog.EndTimeSeconds);

                if (!string.IsNullOrWhiteSpace(dialog.NewlyExportedFilePath))
                {
                    _viewModel.AddFiles(new[] { dialog.NewlyExportedFilePath });
                }

                applied = true;
                break;
            }
        }
        finally
        {
            hotkeyErrors = _viewModel.ReapplyHotkeys();
        }

        var relevantErrors = hotkeyErrors
            .Where(error => error.Contains(sound.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (applied && relevantErrors.Count > 0)
        {
            MessageBox.Show(
                this,
                "Ayar kaydedildi ancak bu sesin global kısayolu etkinleştirilemedi:\n\n" +
                string.Join("\n", relevantErrors),
                "Kısayol Uyarısı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        _viewModel.RefreshAvailability();
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
        {
            _viewModel.AddFiles(paths);
        }
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        _viewModel.SaveConfig();
        _viewModel.DetachHotkeyService();
        _hotkeyService.Dispose();
        _microphoneMixer.Dispose();
        _audioEngine.Dispose();
    }

    private void OnEditSoundRequested(object? sender, SoundItemViewModel sound) => EditSound(sound);

    private void OnRemoveSoundRequested(object? sender, SoundItemViewModel sound)
    {
        var result = MessageBox.Show(
            this,
            $"'{sound.Name}' listeden kaldırılsın mı?\n\nDosya diskten silinmez.",
            "Sesi Kaldır",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.RemoveSound(sound);
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape &&
            SearchBox.IsKeyboardFocusWithin &&
            !string.IsNullOrEmpty(_viewModel.SearchText))
        {
            _viewModel.SearchText = string.Empty;
            e.Handled = true;
        }
    }

    private static bool IsInsideButton(object? originalSource)
    {
        var current = originalSource as DependencyObject;

        while (current is not null)
        {
            if (current is Button)
            {
                return true;
            }

            current = current switch
            {
                Visual or Visual3D => VisualTreeHelper.GetParent(current),
                ContentElement content => ContentOperations.GetParent(content)
                    ?? (content as FrameworkContentElement)?.Parent,
                _ => null
            };
        }

        return false;
    }

    private static SoundItemViewModel? GetSoundFromMenuItem(object sender) =>
        (sender as FrameworkElement)?.DataContext as SoundItemViewModel;
}
