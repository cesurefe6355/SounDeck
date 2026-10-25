using System.Text.Json.Serialization;

namespace SoundDeck.Models;

[Flags]
public enum HotkeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008
}

public sealed class HotkeyGesture
{
    public HotkeyModifiers Modifiers { get; set; }

    public int VirtualKey { get; set; }

    [JsonIgnore]
    public string DisplayText => HotkeyFormatter.Format(this);

    [JsonIgnore]
    public string KeyText => HotkeyFormatter.FormatKey(VirtualKey);

    public HotkeyGesture Clone() => new()
    {
        Modifiers = Modifiers,
        VirtualKey = VirtualKey
    };

    public bool Matches(HotkeyGesture? other) =>
        other is not null && Modifiers == other.Modifiers && VirtualKey == other.VirtualKey;

    public override bool Equals(object? obj) => obj is HotkeyGesture other && Matches(other);

    public override int GetHashCode() => HashCode.Combine((uint)Modifiers, VirtualKey);

    public override string ToString() => DisplayText;
}

public static class HotkeyFormatter
{
    public static string Format(HotkeyGesture? gesture)
    {
        if (gesture is null || gesture.VirtualKey == 0)
        {
            return "Kısayol yok";
        }

        var parts = new List<string>(5);

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Win))
        {
            parts.Add("Win");
        }

        parts.Add(FormatKey(gesture.VirtualKey));
        return string.Join(" + ", parts);
    }

    public static string FormatKey(int virtualKey)
    {
        if (virtualKey is >= 0x30 and <= 0x39)
        {
            return ((char)virtualKey).ToString();
        }

        if (virtualKey is >= 0x41 and <= 0x5A)
        {
            return ((char)virtualKey).ToString();
        }

        if (virtualKey is >= 0x60 and <= 0x69)
        {
            return $"Num {(char)('0' + (virtualKey - 0x60))}";
        }

        if (virtualKey is >= 0x70 and <= 0x87)
        {
            return $"F{virtualKey - 0x6F}";
        }

        return virtualKey switch
        {
            0x08 => "Backspace",
            0x09 => "Tab",
            0x0D => "Enter",
            0x1B => "Esc",
            0x20 => "Space",
            0x21 => "Page Up",
            0x22 => "Page Down",
            0x23 => "End",
            0x24 => "Home",
            0x25 => "Left",
            0x26 => "Up",
            0x27 => "Right",
            0x28 => "Down",
            0x2C => "Print Screen",
            0x2D => "Insert",
            0x2E => "Delete",
            0x6A => "Num *",
            0x6B => "Num +",
            0x6D => "Num -",
            0x6E => "Num .",
            0x6F => "Num /",
            0xBA => ";",
            0xBB => "=",
            0xBC => ",",
            0xBD => "-",
            0xBE => ".",
            0xBF => "/",
            0xC0 => "`",
            0xDB => "[",
            0xDC => "\\",
            0xDD => "]",
            0xDE => "'",
            _ => $"VK {virtualKey:X2}"
        };
    }
}
