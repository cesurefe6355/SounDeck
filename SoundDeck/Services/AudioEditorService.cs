using NAudio.Vorbis;
using NAudio.Wave;

namespace SoundDeck.Services;

public static class AudioEditorService
{
    public static WaveStream CreateReader(string filePath)
    {
        if (Path.GetExtension(filePath).Equals(".ogg", StringComparison.OrdinalIgnoreCase))
        {
            return new VorbisWaveReader(filePath);
        }

        return new AudioFileReader(filePath);
    }

    public static TimeSpan GetDuration(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return TimeSpan.Zero;
        }

        try
        {
            using var reader = CreateReader(filePath);
            return reader.TotalTime;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    public static float[] ExtractPeaks(string filePath, int targetPointCount = 500)
    {
        if (!File.Exists(filePath) || targetPointCount <= 0)
        {
            return Array.Empty<float>();
        }

        try
        {
            using var reader = CreateReader(filePath);
            var sampleProvider = reader is ISampleProvider sp ? sp : reader.ToSampleProvider();
            var channels = sampleProvider.WaveFormat.Channels;
            var sampleRate = sampleProvider.WaveFormat.SampleRate;
            var totalSeconds = reader.TotalTime.TotalSeconds;

            if (totalSeconds <= 0)
            {
                return Array.Empty<float>();
            }

            var totalSamples = (long)(totalSeconds * sampleRate * channels);
            var samplesPerPoint = Math.Max(1, (int)(totalSamples / (targetPointCount * channels)));

            var peaks = new float[targetPointCount];
            var buffer = new float[4096 * channels];
            var currentPoint = 0;
            var samplesInCurrentPoint = 0;
            var currentMax = 0f;

            int read;
            while ((read = sampleProvider.Read(buffer, 0, buffer.Length)) > 0 && currentPoint < targetPointCount)
            {
                for (int i = 0; i < read; i += channels)
                {
                    float channelMax = 0f;
                    for (int ch = 0; ch < channels && (i + ch) < read; ch++)
                    {
                        var abs = Math.Abs(buffer[i + ch]);
                        if (abs > channelMax) channelMax = abs;
                    }

                    if (channelMax > currentMax) currentMax = channelMax;
                    samplesInCurrentPoint++;

                    if (samplesInCurrentPoint >= samplesPerPoint)
                    {
                        peaks[currentPoint] = Math.Clamp(currentMax, 0.03f, 1f);
                        currentPoint++;
                        samplesInCurrentPoint = 0;
                        currentMax = 0f;
                        if (currentPoint >= targetPointCount) break;
                    }
                }
            }

            // Doldurulamayan kalan noktalar varsa tamamla
            while (currentPoint < targetPointCount)
            {
                peaks[currentPoint] = currentPoint > 0 ? peaks[currentPoint - 1] * 0.7f : 0.05f;
                currentPoint++;
            }

            return peaks;
        }
        catch
        {
            return Array.Empty<float>();
        }
    }

    public static void TrimToWav(string sourcePath, string destinationPath, TimeSpan start, TimeSpan end)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Kaynak ses dosyası bulunamadı.", sourcePath);
        }

        using var reader = CreateReader(sourcePath);
        var total = reader.TotalTime;

        if (start < TimeSpan.Zero)
        {
            start = TimeSpan.Zero;
        }

        if (end <= TimeSpan.Zero || end > total)
        {
            end = total;
        }

        if (end <= start)
        {
            throw new InvalidOperationException("Bitiş süresi başlangıç süresinden büyük olmalıdır.");
        }

        if (start > TimeSpan.Zero)
        {
            try
            {
                reader.CurrentTime = start;
            }
            catch
            {
                // Seeking fallback
            }
        }

        var sampleProvider = reader is ISampleProvider sp
            ? sp
            : reader.ToSampleProvider();

        var duration = end - start;
        var trimmed = new TrimmedSampleProvider(sampleProvider, duration);

        var destinationDir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        WaveFileWriter.CreateWaveFile16(destinationPath, trimmed);
    }
}
