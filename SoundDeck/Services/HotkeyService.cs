using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using SoundDeck.Models;

namespace SoundDeck.Services;

public sealed class HotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModNoRepeat = 0x4000;
    private const int BaseHotkeyId = 0x5344;
    private const int HotkeyDebounceMilliseconds = 120;

    private readonly Dictionary<Guid, HotkeyRegistration> _registrations = new();
    private readonly Dictionary<int, Guid> _idToSound = new();
    private HwndSource? _source;
    private IntPtr _windowHandle;
    private int _nextId = BaseHotkeyId;
    private Guid _lastHotkeySoundId;
    private long _lastHotkeyAt;
    private bool _disposed;

    public event EventHandler<Guid>? HotkeyPressed;

    public bool IsInitialized => _windowHandle != IntPtr.Zero;

    public void Initialize(Window window)
    {
        if (_windowHandle != IntPtr.Zero)
        {
            return;
        }

        var helper = new WindowInteropHelper(window);
        _windowHandle = helper.Handle;

        if (_windowHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Kısayollar için pencere tanıtıcısı oluşturulamadı.");
        }

        _source = HwndSource.FromHwnd(_windowHandle);

        if (_source is null)
        {
            throw new InvalidOperationException("Kısayol mesaj kaynağı oluşturulamadı.");
        }

        _source.AddHook(WindowProc);
    }

    public HotkeyRegistrationResult Register(Guid soundId, HotkeyGesture gesture)
    {
        if (_windowHandle == IntPtr.Zero)
        {
            return HotkeyRegistrationResult.Failure("Pencere henüz kısayol kaydı için hazır değil.");
        }

        if (gesture.VirtualKey <= 0)
        {
            return HotkeyRegistrationResult.Failure("Geçersiz kısayol tuşu.");
        }

        if (_registrations.Any(pair =>
                pair.Key != soundId && pair.Value.Gesture.Matches(gesture)))
        {
            return HotkeyRegistrationResult.Failure(
                "Bu kısayol zaten başka bir sese atanmış.");
        }

        Unregister(soundId);

        var id = _nextId++;

        if (id < 0)
        {
            _nextId = BaseHotkeyId + 1;
            id = BaseHotkeyId;
        }

        var modifiers = (uint)gesture.Modifiers | ModNoRepeat;

        if (!RegisterHotKey(_windowHandle, id, modifiers, (uint)gesture.VirtualKey))
        {
            var errorCode = Marshal.GetLastWin32Error();
            var message = errorCode == 1409
                ? "Bu kısayol başka bir uygulama tarafından kullanılıyor."
                : new Win32Exception(errorCode).Message;

            return HotkeyRegistrationResult.Failure(message);
        }

        _registrations[soundId] = new HotkeyRegistration(id, gesture.Clone());
        _idToSound[id] = soundId;
        return HotkeyRegistrationResult.Success();
    }

    public void Unregister(Guid soundId)
    {
        if (!_registrations.TryGetValue(soundId, out var registration))
        {
            return;
        }

        if (_windowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(_windowHandle, registration.Id);
        }

        _registrations.Remove(soundId);
        _idToSound.Remove(registration.Id);
    }

    public void UnregisterAll()
    {
        foreach (var registration in _registrations.Values.ToList())
        {
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, registration.Id);
            }
        }

        _registrations.Clear();
        _idToSound.Clear();
        _lastHotkeySoundId = Guid.Empty;
        _lastHotkeyAt = 0;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        UnregisterAll();

        if (_source is not null)
        {
            _source.RemoveHook(WindowProc);
            _source = null;
        }

        _windowHandle = IntPtr.Zero;
    }

    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && _idToSound.TryGetValue(wParam.ToInt32(), out var soundId))
        {
            handled = true;

            var now = Environment.TickCount64;

            if (soundId == _lastHotkeySoundId &&
                now - _lastHotkeyAt < HotkeyDebounceMilliseconds)
            {
                return IntPtr.Zero;
            }

            _lastHotkeySoundId = soundId;
            _lastHotkeyAt = now;
            HotkeyPressed?.Invoke(this, soundId);
        }

        return IntPtr.Zero;
    }

    private sealed record HotkeyRegistration(int Id, HotkeyGesture Gesture);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

public sealed record HotkeyRegistrationResult(bool IsSuccess, string? Error)
{
    public static HotkeyRegistrationResult SuccessResult { get; } = new(true, null);

    public static HotkeyRegistrationResult Failure(string error) => new(false, error);

    public static HotkeyRegistrationResult Success() => SuccessResult;
}
