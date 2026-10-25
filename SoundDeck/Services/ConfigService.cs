using System.Text.Json;
using SoundDeck.Models;

namespace SoundDeck.Services;

public sealed class ConfigService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public ConfigService(string? configPath = null)
    {
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            ConfigPath = configPath;
            return;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        ConfigPath = Path.Combine(appData, "SoundDeck", "sounddeck_config.json");
    }

    public string ConfigPath { get; }

    public ConfigLoadResult Load()
    {
        if (!File.Exists(ConfigPath))
        {
            return new ConfigLoadResult(new AppConfig(), null);
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? new AppConfig();
            Normalize(config);
            return new ConfigLoadResult(config, null);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            var warning = $"Ayar dosyası okunamadı. Yedeklenip boş bir liste ile başlatıldı. ({ex.Message})";

            try
            {
                var backupPath = ConfigPath + $".bozuk-{DateTime.Now:yyyyMMdd-HHmmss}.bak";
                File.Copy(ConfigPath, backupPath, overwrite: true);
                warning += $" Yedek: {backupPath}";
            }
            catch
            {
                // Yedekleme başarısız olsa da uygulama açılmaya devam etmeli.
            }

            return new ConfigLoadResult(new AppConfig(), warning);
        }
    }

    public void Save(AppConfig config)
    {
        var directory = Path.GetDirectoryName(ConfigPath)
            ?? throw new InvalidOperationException("Ayar dosyası klasörü belirlenemedi.");

        Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(config, _jsonOptions);
        var tempPath = ConfigPath + ".tmp";

        try
        {
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, ConfigPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // Geçici dosya sonraki kayıtta tekrar denenecek.
                }
            }
        }
    }

    private static void Normalize(AppConfig config)
    {
        config.Output ??= new OutputSettings();
        config.Microphone ??= new MicrophoneSettings();
        config.Sounds = (config.Sounds ?? new List<SoundItem>())
            .Where(sound => sound is not null)
            .ToList();

        foreach (var sound in config.Sounds)
        {
            sound.FilePath ??= string.Empty;
            sound.Name ??= string.Empty;

            if (sound.Id == Guid.Empty)
            {
                sound.Id = Guid.NewGuid();
            }

            sound.Name = string.IsNullOrWhiteSpace(sound.Name)
                ? Path.GetFileNameWithoutExtension(sound.FilePath)
                : sound.Name.Trim();

            sound.Volume = Math.Clamp(sound.Volume, 0, 1);

            if (sound.Hotkey is { VirtualKey: <= 0 })
            {
                sound.Hotkey = null;
            }
        }
    }
}

public sealed record ConfigLoadResult(AppConfig Config, string? Warning);
