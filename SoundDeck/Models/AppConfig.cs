namespace SoundDeck.Models;

public sealed class AppConfig
{
    public int Version { get; set; } = 1;

    public OutputSettings Output { get; set; } = new();

    public MicrophoneSettings Microphone { get; set; } = new();

    public List<SoundItem> Sounds { get; set; } = new();
}

public sealed class OutputSettings
{
    public string? MonitorDeviceId { get; set; }

    public bool MonitorEnabled { get; set; } = true;

    public string? BroadcastDeviceId { get; set; }

    public bool BroadcastEnabled { get; set; }
}

public sealed class MicrophoneSettings
{
    public string? DeviceId { get; set; }

    public bool Enabled { get; set; } = true;
}

public sealed class AudioDevice
{
    public AudioDevice(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}
