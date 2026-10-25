using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SoundDeck.Services;

/// <summary>
/// Fiziksel mikrofonu seçili sanal mikrofon çıkışına (ör. CABLE Input) sürekli aktarır.
/// SoundDeck'in çaldığı sesler de aynı çıkışa gittiği için Windows iki akışı birleştirir.
/// </summary>
public sealed class MicrophoneMixer : IDisposable
{
    private const int CaptureBufferMilliseconds = 50;
    private const int OutputLatencyMilliseconds = 50;
    private const int MixBufferMilliseconds = 250;
    private static readonly Guid FloatSubFormat = new("00000003-0000-0010-8000-00aa00389b71");

    private readonly object _gate = new();
    private readonly AudioDeviceService _deviceService;
    private WasapiCapture? _capture;
    private WasapiOut? _output;
    private BufferedWaveProvider? _buffer;
    private MediaFoundationResampler? _resampler;
    private MMDevice? _captureDevice;
    private MMDevice? _outputDevice;
    private string? _captureDeviceId;
    private string? _outputDeviceId;
    private bool _disposed;

    public MicrophoneMixer(AudioDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public string? CaptureDeviceId => _captureDeviceId;

    public string? OutputDeviceId => _outputDeviceId;

    public string? LastError { get; private set; }

    public bool IsRunning
    {
        get
        {
            lock (_gate)
            {
                return _capture is not null &&
                       _output is not null &&
                       _capture.CaptureState == CaptureState.Capturing &&
                       _output.PlaybackState == PlaybackState.Playing;
            }
        }
    }

    public bool Start(string captureDeviceId, string outputDeviceId)
    {
        lock (_gate)
        {
            if (_disposed)
            {
                LastError = "Mikrofon karıştırıcı kapatılmış.";
                return false;
            }

            if (string.Equals(_captureDeviceId, captureDeviceId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_outputDeviceId, outputDeviceId, StringComparison.OrdinalIgnoreCase) &&
                IsRunningCore())
            {
                LastError = null;
                return true;
            }

            StopCore();

            MMDevice? captureDevice = null;
            MMDevice? outputDevice = null;
            WasapiCapture? capture = null;
            WasapiOut? output = null;
            BufferedWaveProvider? buffer = null;
            MediaFoundationResampler? resampler = null;

            try
            {
                captureDevice = _deviceService.OpenCaptureDevice(captureDeviceId);
                outputDevice = _deviceService.OpenRenderDevice(outputDeviceId);

                capture = new WasapiCapture(
                    captureDevice,
                    useEventSync: true,
                    audioBufferMillisecondsLength: CaptureBufferMilliseconds);

                var captureFormat = capture.WaveFormat;
                buffer = new BufferedWaveProvider(captureFormat)
                {
                    BufferDuration = TimeSpan.FromMilliseconds(MixBufferMilliseconds),
                    DiscardOnBufferOverflow = true,
                    ReadFully = true
                };

                var sampleBuffer = buffer;
                capture.DataAvailable += (_, args) =>
                {
                    try
                    {
                        sampleBuffer.AddSamples(args.Buffer, 0, args.BytesRecorded);
                    }
                    catch
                    {
                        // Karıştırıcı kapatılırken tampon boşaltılmış olabilir.
                    }
                };

                capture.RecordingStopped += (_, args) =>
                {
                    if (args.Exception is not null)
                    {
                        LastError = args.Exception.Message;
                    }
                };

                var targetFormat = outputDevice.AudioClient.MixFormat;
                IWaveProvider provider = buffer;

                if (!AreFormatsCompatible(captureFormat, targetFormat))
                {
                    resampler = new MediaFoundationResampler(buffer, targetFormat)
                    {
                        ResamplerQuality = 60
                    };
                    provider = resampler;
                }

                output = new WasapiOut(
                    outputDevice,
                    AudioClientShareMode.Shared,
                    useEventSync: true,
                    OutputLatencyMilliseconds);

                output.Init(provider);
                output.Play();
                capture.StartRecording();

                _captureDevice = captureDevice;
                _outputDevice = outputDevice;
                _capture = capture;
                _output = output;
                _buffer = buffer;
                _resampler = resampler;
                _captureDeviceId = captureDeviceId;
                _outputDeviceId = outputDeviceId;
                LastError = null;
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;

                try
                {
                    capture?.StopRecording();
                }
                catch
                {
                    // Kayıt zaten durmuş olabilir.
                }

                try
                {
                    output?.Stop();
                }
                catch
                {
                    // Çıkış zaten durmuş olabilir.
                }

                try
                {
                    output?.Dispose();
                }
                catch
                {
                    // Ses aygıtı kaldırılmış olabilir.
                }

                try
                {
                    capture?.Dispose();
                }
                catch
                {
                    // Kayıt aygıtı kaldırılmış olabilir.
                }

                try
                {
                    resampler?.Dispose();
                }
                catch
                {
                    // Dönüştürücü zaten kapatılmış olabilir.
                }

                try
                {
                    buffer?.ClearBuffer();
                }
                catch
                {
                    // Tampon zaten boşaltılmış olabilir.
                }

                try
                {
                    captureDevice?.Dispose();
                }
                catch
                {
                    // COM nesnesi zaten serbest bırakılmış olabilir.
                }

                try
                {
                    outputDevice?.Dispose();
                }
                catch
                {
                    // COM nesnesi zaten serbest bırakılmış olabilir.
                }

                return false;
            }
        }
    }

    public void Stop()
    {
        lock (_gate)
        {
            StopCore();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            StopCore();
        }
    }

    private bool IsRunningCore() =>
        _capture is not null &&
        _output is not null &&
        _capture.CaptureState == CaptureState.Capturing &&
        _output.PlaybackState == PlaybackState.Playing;

    private void StopCore()
    {
        var capture = _capture;
        var output = _output;
        var buffer = _buffer;
        var resampler = _resampler;
        var captureDevice = _captureDevice;
        var outputDevice = _outputDevice;

        _capture = null;
        _output = null;
        _buffer = null;
        _resampler = null;
        _captureDevice = null;
        _outputDevice = null;
        _captureDeviceId = null;
        _outputDeviceId = null;

        try
        {
            capture?.StopRecording();
        }
        catch
        {
            // Kayıt zaten durmuş olabilir.
        }

        try
        {
            output?.Stop();
        }
        catch
        {
            // Çıkış zaten durmuş olabilir.
        }

        try
        {
            output?.Dispose();
        }
        catch
        {
            // Ses aygıtı kaldırılmış olabilir.
        }

        try
        {
            capture?.Dispose();
        }
        catch
        {
            // Kayıt aygıtı kaldırılmış olabilir.
        }

        try
        {
            resampler?.Dispose();
        }
        catch
        {
            // Dönüştürücü zaten kapatılmış olabilir.
        }

        try
        {
            buffer?.ClearBuffer();
        }
        catch
        {
            // Tampon zaten boşaltılmış olabilir.
        }

        try
        {
            captureDevice?.Dispose();
        }
        catch
        {
            // COM nesnesi zaten serbest bırakılmış olabilir.
        }

        try
        {
            outputDevice?.Dispose();
        }
        catch
        {
            // COM nesnesi zaten serbest bırakılmış olabilir.
        }
    }

    private static bool AreFormatsCompatible(WaveFormat source, WaveFormat target)
    {
        return source.SampleRate == target.SampleRate &&
               source.Channels == target.Channels &&
               source.BitsPerSample == target.BitsPerSample &&
               IsFloat(source) == IsFloat(target);
    }

    private static bool IsFloat(WaveFormat format)
    {
        if (format.Encoding == WaveFormatEncoding.IeeeFloat)
        {
            return true;
        }

        return format is WaveFormatExtensible extensible &&
               extensible.SubFormat == FloatSubFormat;
    }
}
