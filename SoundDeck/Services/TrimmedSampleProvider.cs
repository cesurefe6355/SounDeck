using NAudio.Wave;

namespace SoundDeck.Services;

public sealed class TrimmedSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly long _totalSamplesToRead;
    private long _samplesRead;

    public TrimmedSampleProvider(ISampleProvider source, TimeSpan duration)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        WaveFormat = source.WaveFormat;
        _totalSamplesToRead = Math.Max(0, (long)(duration.TotalSeconds * source.WaveFormat.SampleRate * source.WaveFormat.Channels));
    }

    public WaveFormat WaveFormat { get; }

    public int Read(float[] buffer, int offset, int count)
    {
        if (_samplesRead >= _totalSamplesToRead)
        {
            return 0;
        }

        var remaining = _totalSamplesToRead - _samplesRead;
        var toRead = (int)Math.Min(count, remaining);
        var read = _source.Read(buffer, offset, toRead);
        _samplesRead += read;

        if (read == 0)
        {
            _samplesRead = _totalSamplesToRead;
        }

        return read;
    }
}
