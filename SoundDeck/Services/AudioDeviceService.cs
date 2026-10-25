using Microsoft.Win32;
using NAudio.CoreAudioApi;
using SoundDeck.Models;

namespace SoundDeck.Services;

public sealed class AudioDeviceService
{
    private const string ListenTargetProperty = "{24dbb0fc-9311-4b3d-9cf0-18ff155639d4},0";
    private const string ListenEnabledProperty = "{24dbb0fc-9311-4b3d-9cf0-18ff155639d4},1";

    public IReadOnlyList<AudioDevice> GetRenderDevices()
    {
        var devices = new List<AudioDevice>();
        string? defaultDeviceId = null;

        try
        {
            using var enumerator = new MMDeviceEnumerator();

            try
            {
                using var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                defaultDeviceId = defaultDevice.ID;
            }
            catch
            {
                // Varsayılan cihaz olmasa da diğer çıkışları listelemeye devam et.
            }

            var collection = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            var nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var device in collection)
            {
                try
                {
                    var name = device.FriendlyName;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = device.DeviceFriendlyName;
                    }

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = "İsimsiz çıkış cihazı";
                    }

                    nameCounts.TryGetValue(name, out var count);
                    count++;
                    nameCounts[name] = count;

                    var uniqueName = count > 1 ? $"{name} ({count})" : name;
                    var hint = GetDeviceHint(name);
                    var displayName = hint is null ? uniqueName : $"{uniqueName}  •  {hint}";
                    devices.Add(new AudioDevice(device.ID, displayName));
                }
                catch
                {
                    // Tek bir cihaz okunamazsa diğerlerini engelleme.
                }
                finally
                {
                    device.Dispose();
                }
            }

            return devices
                .OrderByDescending(device => device.Id == defaultDeviceId)
                .ThenBy(device => device.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        catch
        {
            return devices;
        }
    }

    public IReadOnlyList<AudioDevice> GetCaptureDevices()
    {
        var devices = new List<AudioDevice>();
        string? defaultDeviceId = null;

        try
        {
            using var enumerator = new MMDeviceEnumerator();

            try
            {
                using var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                defaultDeviceId = defaultDevice.ID;
            }
            catch
            {
                try
                {
                    using var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                    defaultDeviceId = defaultDevice.ID;
                }
                catch
                {
                    // Varsayılan mikrofon olmasa da diğer girişleri listelemeye devam et.
                }
            }

            var collection = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var device in collection)
            {
                try
                {
                    var name = device.FriendlyName;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = device.DeviceFriendlyName;
                    }

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = "İsimsiz mikrofon";
                    }

                    if (IsVirtualCaptureDevice(name))
                    {
                        continue;
                    }

                    nameCounts.TryGetValue(name, out var count);
                    count++;
                    nameCounts[name] = count;

                    var uniqueName = count > 1 ? $"{name} ({count})" : name;
                    var hint = GetCaptureDeviceHint(name);
                    var displayName = hint is null ? uniqueName : $"{uniqueName}  •  {hint}";
                    devices.Add(new AudioDevice(device.ID, displayName));
                }
                catch
                {
                    // Tek bir cihaz okunamazsa diğerlerini engelleme.
                }
                finally
                {
                    device.Dispose();
                }
            }

            return devices
                .OrderByDescending(device => device.Id == defaultDeviceId)
                .ThenBy(device => device.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        catch
        {
            return devices;
        }
    }

    private static bool IsVirtualCaptureDevice(string name)
    {
        var lower = name.ToLowerInvariant();

        return lower.Contains("cable output") ||
               lower.Contains("cable-a output") ||
               lower.Contains("cable-b output") ||
               lower.Contains("voicemeeter output") ||
               lower.Contains("voicemeeter out") ||
               lower.Contains("sonar") ||
               lower.Contains("stereo mix") ||
               lower.Contains("what u hear") ||
               lower.Contains("wave out mix") ||
               lower.Contains("loopback");
    }

    private static string? GetCaptureDeviceHint(string name)
    {
        var lower = name.ToLowerInvariant();

        if (lower.Contains("microphone") ||
            lower.Contains("mikrofon") ||
            lower.Contains("headset") ||
            lower.Contains("mic ("))
        {
            return "mikrofon";
        }

        if (lower.Contains("line in") ||
            lower.Contains("line girişi") ||
            lower.Contains("line input"))
        {
            return "hat girişi";
        }

        return "ses girişi";
    }

    private static string? GetDeviceHint(string name)
    {
        var lower = name.ToLowerInvariant();

        if (lower.Contains("cable input") ||
            lower.Contains("cable-a input") ||
            lower.Contains("cable-b input") ||
            lower.Contains("vb-cable"))
        {
            return "sanal mikrofon (VB-CABLE)";
        }

        if (lower.Contains("voicemeeter"))
        {
            return "sanal mikrofon (Voicemeeter)";
        }

        if (lower.Contains("vb-audio"))
        {
            return "sanal mikrofon (VB-CABLE)";
        }

        if (lower.Contains("nvidia") &&
            (lower.Contains("virtual") || lower.Contains("broadcast")))
        {
            return "sanal mikrofon (NVIDIA)";
        }

        if (lower.Contains("virtual audio cable") ||
            lower.Contains("virtual cable") ||
            lower.Contains("virtual audio device"))
        {
            return "sanal mikrofon";
        }

        if (lower.Contains("sonar - microphone") || lower.Contains("sonar - stream"))
        {
            return "Sonar sanal mikrofon";
        }

        if (lower.Contains("sonar - gaming") ||
            lower.Contains("sonar - media") ||
            lower.Contains("sonar - chat") ||
            lower.Contains("sonar - aux"))
        {
            return "Sonar kanalı";
        }

        return null;
    }

    public bool IsCaptureDeviceAvailable(params string[] nameFragments)
    {
        if (nameFragments.Length == 0)
        {
            return false;
        }

        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var collection = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            foreach (var device in collection)
            {
                try
                {
                    foreach (var fragment in nameFragments)
                    {
                        if (string.IsNullOrWhiteSpace(fragment))
                        {
                            continue;
                        }

                        if (device.FriendlyName.Contains(
                                fragment,
                                StringComparison.CurrentCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    // Tek bir cihaz okunamazsa diğerlerini engelleme.
                }
                finally
                {
                    device.Dispose();
                }
            }
        }
        catch
        {
            // Ses altyapısı geçici olarak kullanılamıyor olabilir.
        }

        return false;
    }

    public string? GetDefaultRenderDeviceId()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device.ID;
        }
        catch
        {
            return null;
        }
    }

    public string? GetDefaultCaptureDeviceId()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();

            foreach (var role in new[] { Role.Communications, Role.Multimedia })
            {
                try
                {
                    using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, role);

                    if (device.State == DeviceState.Active)
                    {
                        return device.ID;
                    }
                }
                catch
                {
                    // Bu rol için varsayılan cihaz yoksa diğerini dene.
                }
            }
        }
        catch
        {
            // Ses altyapısı geçici olarak kullanılamıyor olabilir.
        }

        return null;
    }

    public MMDevice OpenCaptureDevice(string deviceId)
    {
        var enumerator = new MMDeviceEnumerator();

        try
        {
            var device = enumerator.GetDevice(deviceId);

            if (device.State != DeviceState.Active || device.DataFlow != DataFlow.Capture)
            {
                device.Dispose();
                throw new InvalidOperationException("Seçili mikrofon artık kullanılamıyor.");
            }

            return device;
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    public MMDevice OpenRenderDevice(string deviceId)
    {
        var enumerator = new MMDeviceEnumerator();

        try
        {
            var device = enumerator.GetDevice(deviceId);

            if (device.State != DeviceState.Active)
            {
                device.Dispose();
                throw new InvalidOperationException("Seçili çıkış cihazı artık kullanılamıyor.");
            }

            return device;
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    /// <summary>
    /// Windows'un "Bu cihazı dinle" ayarı bu mikrofon için açık mı ve seçili hedefe mi gidiyor?
    /// Açıksa SoundDeck kendi karıştırıcısını başlatmayarak mikrofonun iki kez gitmesini önler.
    /// </summary>
    public bool IsListenToDeviceEnabled(string captureDeviceId, string targetDeviceId)
    {
        if (string.IsNullOrWhiteSpace(captureDeviceId) ||
            string.IsNullOrWhiteSpace(targetDeviceId))
        {
            return false;
        }

        try
        {
            var endpointGuid = GetCaptureEndpointGuid(captureDeviceId);

            if (endpointGuid is null)
            {
                return false;
            }

            using var key = Registry.LocalMachine.OpenSubKey(
                $@"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture\{endpointGuid}\Properties");

            if (key is null)
            {
                return false;
            }

            if (key.GetValue(ListenTargetProperty) is not string target ||
                !string.Equals(target, targetDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return key.GetValue(ListenEnabledProperty) is byte[] enabled &&
                   IsPropVariantTrue(enabled);
        }
        catch
        {
            // Kayıt defteri okunamıyorsa ses akışını engelleme.
            return false;
        }
    }

    private static string? GetCaptureEndpointGuid(string deviceId)
    {
        var lastDot = deviceId.LastIndexOf('.');

        if (lastDot < 0 || lastDot + 1 >= deviceId.Length)
        {
            return null;
        }

        var guid = deviceId[(lastDot + 1)..];
        return guid.StartsWith('{') && guid.EndsWith('}') ? guid : null;
    }

    private static bool IsPropVariantTrue(byte[] value)
    {
        if (value.Length < 6)
        {
            return false;
        }

        const ushort vtBool = 11;

        return BitConverter.ToUInt16(value, 0) == vtBool &&
               BitConverter.ToInt16(value, 4) != 0;
    }
}
