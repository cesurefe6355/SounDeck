using System.Windows.Threading;
using NAudio.CoreAudioApi;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundDeck.Models;

namespace SoundDeck.Services;

public sealed class AudioEngine : IDisposable
{
    private const int MaxConcurrentSessions = 64;
    private const int OutputLatencyMilliseconds = 80;

    private readonly object _gate = new();
    private readonly List<PlaybackSession> _activeSessions = new();
    private readonly AudioDeviceService _deviceService;
    private readonly Dispatcher _dispatcher;
    private bool _disposed;

    public AudioEngine(AudioDeviceService deviceService)
    {
        _deviceService = deviceService;
        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    public event EventHandler<PlaybackEventArgs>? PlaybackStarted;

    public event EventHandler<PlaybackEventArgs>? PlaybackStopped;

    public event EventHandler<string>? PlaybackFailed;

    public int Play(SoundItem sound, IEnumerable<string> deviceIds)
    {
        if (_disposed)
        {
            return 0;
        }

        if (!File.Exists(sound.FilePath))
        {
            DispatchFailure($"Dosya bulunamadı: {sound.FilePath}");
            return 0;
        }

        var targets = deviceIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (targets.Count == 0)
        {
            DispatchFailure("Etkin bir çıkış cihazı seçili değil.");
            return 0;
        }

        var startedCount = 0;

        foreach (var deviceId in targets)
        {
            lock (_gate)
            {
                if (_activeSessions.Count >= MaxConcurrentSessions)
                {
                    DispatchFailure("Aynı anda çalınabilecek maksimum ses sayısına ulaşıldı.");
                    return startedCount;
                }
            }

            PlaybackSession? session = null;

            try
            {
                var device = _deviceService.OpenRenderDevice(deviceId);
                var reader = CreateReader(sound.FilePath);
                var totalDuration = reader.TotalTime;

                if (sound.StartTimeSeconds > 0.02 && sound.StartTimeSeconds < totalDuration.TotalSeconds)
                {
                    try
                    {
                        reader.CurrentTime = TimeSpan.FromSeconds(sound.StartTimeSeconds);
                    }
                    catch
                    {
                        // Seeking fallback
                    }
                }

                var sampleProvider = reader is ISampleProvider provider
                    ? provider
                    : reader.ToSampleProvider();

                if (sound.EndTimeSeconds.HasValue && sound.EndTimeSeconds.Value > sound.StartTimeSeconds)
                {
                    var endSec = Math.Min(totalDuration.TotalSeconds, sound.EndTimeSeconds.Value);
                    var durationSec = endSec - Math.Max(0, sound.StartTimeSeconds);
                    if (durationSec > 0.02)
                    {
                        sampleProvider = new TrimmedSampleProvider(sampleProvider, TimeSpan.FromSeconds(durationSec));
                    }
                }

                var volumeProvider = new VolumeSampleProvider(sampleProvider)
                {
                    Volume = (float)Math.Clamp(sound.Volume, 0, 1)
                };

                var output = new WasapiOut(
                    device,
                    AudioClientShareMode.Shared,
                    useEventSync: true,
                    OutputLatencyMilliseconds);

                session = new PlaybackSession(sound.Id, deviceId, device, reader, output);
                output.PlaybackStopped += (_, args) => HandlePlaybackStopped(session, args.Exception);
                output.Init(volumeProvider);

                lock (_gate)
                {
                    _activeSessions.Add(session);
                }

                try
                {
                    output.Play();
                }
                catch
                {
                    lock (_gate)
                    {
                        _activeSessions.Remove(session);
                    }

                    session.Dispose();
                    throw;
                }

                startedCount++;
                DispatchPlaybackStarted(session);
            }
            catch (Exception ex)
            {
                session?.Dispose();
                DispatchFailure($"Ses başlatılamadı ({Path.GetFileName(sound.FilePath)}): {ex.Message}");
            }
        }

        return startedCount;
    }

    public void StopSound(Guid soundId)
    {
        List<PlaybackSession> sessions;

        lock (_gate)
        {
            sessions = _activeSessions
                .Where(session => session.SoundId == soundId)
                .ToList();

            foreach (var session in sessions)
            {
                _activeSessions.Remove(session);
            }
        }

        StopSessions(sessions);
    }

    public void StopAll()
    {
        List<PlaybackSession> sessions;

        lock (_gate)
        {
            sessions = _activeSessions.ToList();
            _activeSessions.Clear();
        }

        StopSessions(sessions);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopAll();
    }

    private void StopSessions(IEnumerable<PlaybackSession> sessions)
    {
        foreach (var session in sessions)
        {
            session.Dispose();
            DispatchPlaybackStopped(session);
        }
    }

    private void HandlePlaybackStopped(PlaybackSession session, Exception? exception)
    {
        bool wasActive;

        lock (_gate)
        {
            wasActive = _activeSessions.Remove(session);
        }

        session.Dispose();

        if (!wasActive)
        {
            return;
        }

        if (exception is not null)
        {
            DispatchFailure($"Çalma sırasında hata oluştu: {exception.Message}");
        }

        DispatchPlaybackStopped(session);
    }

    private void DispatchPlaybackStarted(PlaybackSession session)
    {
        Dispatch(() => PlaybackStarted?.Invoke(
            this,
            new PlaybackEventArgs(session.SoundId, session.DeviceId)));
    }

    private void DispatchPlaybackStopped(PlaybackSession session)
    {
        Dispatch(() => PlaybackStopped?.Invoke(
            this,
            new PlaybackEventArgs(session.SoundId, session.DeviceId)));
    }

    private void DispatchFailure(string message)
    {
        Dispatch(() => PlaybackFailed?.Invoke(this, message));
    }

    private void Dispatch(Action action)
    {
        if (_dispatcher.HasShutdownStarted || _dispatcher.HasShutdownFinished)
        {
            return;
        }

        if (_dispatcher.CheckAccess())
        {
            action();
            return;
        }

        _dispatcher.BeginInvoke(DispatcherPriority.Background, action);
    }

    private static WaveStream CreateReader(string filePath)
    {
        if (Path.GetExtension(filePath).Equals(".ogg", StringComparison.OrdinalIgnoreCase))
        {
            return new VorbisWaveReader(filePath);
        }

        return new AudioFileReader(filePath);
    }

    private sealed class PlaybackSession : IDisposable
    {
        private int _disposed;

        public PlaybackSession(
            Guid soundId,
            string deviceId,
            MMDevice device,
            WaveStream reader,
            WasapiOut output)
        {
            SoundId = soundId;
            DeviceId = deviceId;
            Device = device;
            Reader = reader;
            Output = output;
        }

        public Guid SoundId { get; }

        public string DeviceId { get; }

        private MMDevice Device { get; }

        private WaveStream Reader { get; }

        private WasapiOut Output { get; }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            try
            {
                Output.Stop();
            }
            catch
            {
                // Çalma zaten durmuş olabilir.
            }

            try
            {
                Output.Dispose();
            }
            catch
            {
                // Ses aygıtı kaldırılmış olabilir.
            }

            try
            {
                Reader.Dispose();
            }
            catch
            {
                // Kaynak dosya kapanmış olabilir.
            }

            try
            {
                Device.Dispose();
            }
            catch
            {
                // COM nesnesi zaten serbest bırakılmış olabilir.
            }
        }
    }
}

public sealed class PlaybackEventArgs : EventArgs
{
    public PlaybackEventArgs(Guid soundId, string deviceId)
    {
        SoundId = soundId;
        DeviceId = deviceId;
    }

    public Guid SoundId { get; }

    public string DeviceId { get; }
}
