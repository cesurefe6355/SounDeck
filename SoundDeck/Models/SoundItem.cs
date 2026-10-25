namespace SoundDeck.Models;

public sealed class SoundItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public double Volume { get; set; } = 0.85;

    public HotkeyGesture? Hotkey { get; set; }

    public double StartTimeSeconds { get; set; } = 0;

    public double? EndTimeSeconds { get; set; }

    public SoundItem Clone() => new()
    {
        Id = Id,
        Name = Name,
        FilePath = FilePath,
        Volume = Volume,
        Hotkey = Hotkey?.Clone(),
        StartTimeSeconds = StartTimeSeconds,
        EndTimeSeconds = EndTimeSeconds
    };
}
