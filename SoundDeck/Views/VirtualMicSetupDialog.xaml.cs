using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SoundDeck.Services;
using SoundDeck.ViewModels;

namespace SoundDeck.Views;

public partial class VirtualMicSetupDialog : Window
{
    private const string DownloadUrl = "https://vb-audio.com/Cable/";
    private readonly MainViewModel _viewModel;

    public VirtualMicSetupDialog(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();

        SourceInitialized += (_, _) => WindowChromeHelper.ApplyDarkTitleBar(this);
        Loaded += (_, _) => RefreshStatus(autoSelect: false);
    }

    private void OpenDownload_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = DownloadUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"İndirme sayfası açılamadı: {ex.Message}\n\nAdres: {DownloadUrl}",
                "VB-CABLE",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshDevices();
        RefreshStatus(autoSelect: true);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void RefreshStatus(bool autoSelect)
    {
        var hasInput = _viewModel.HasVirtualCableDevice;
        var hasOutput = _viewModel.HasVirtualCableCapture;

        SetStatus(
            CableInputStatusIcon,
            CableInputStatusText,
            hasInput,
            hasInput ? "CABLE girişi bulundu" : "CABLE girişi bulunamadı");

        SetStatus(
            CableOutputStatusIcon,
            CableOutputStatusText,
            hasOutput,
            hasOutput ? "CABLE Output mikrofonu bulundu" : "CABLE Output mikrofonu bulunamadı");

        if (!hasInput)
        {
            InstallStatusText.Text = "VB-CABLE henüz kurulu değil. Kurulumdan sonra Yenile'ye basın.";
            InstallStatusText.Foreground = (Brush)FindResource("TextSecondaryBrush");
            return;
        }

        if (autoSelect)
        {
            _viewModel.UseVirtualCableDevice();
        }

        if (!autoSelect)
        {
            InstallStatusText.Text = hasOutput
                ? "CABLE girişi ve CABLE Output bulundu. B çıkışına uygulamak için Yenile'ye basın."
                : "CABLE girişi bulundu ancak CABLE Output mikrofonu görünmüyor. Windows ses ayarlarından etkinleştirin veya VB-CABLE'i yeniden kurun.";
            InstallStatusText.Foreground = (Brush)FindResource(hasOutput ? "SuccessBrush" : "WarningBrush");
            return;
        }

        InstallStatusText.Text = hasOutput
            ? "Hazır. B çıkışı CABLE girişi olarak ayarlandı; Discord'da mikrofon olarak CABLE Output seçin."
            : "CABLE girişi bulundu ve B çıkışı seçildi. CABLE Output görünmüyorsa Windows ses ayarlarından etkinleştirin veya VB-CABLE'i yeniden kurun.";
        InstallStatusText.Foreground = (Brush)FindResource(hasOutput ? "SuccessBrush" : "WarningBrush");
    }

    private void SetStatus(TextBlock icon, TextBlock text, bool ok, string message)
    {
        icon.Text = ok ? "\uE73E" : "\uE7BA";
        icon.Foreground = (Brush)FindResource(ok ? "SuccessBrush" : "WarningBrush");
        text.Text = message;
        text.Foreground = (Brush)FindResource(ok ? "TextPrimaryBrush" : "TextSecondaryBrush");
    }
}
