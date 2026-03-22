using NotepadDemo.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotepadDemo.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };


        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                    return new AppSettings();

                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            catch (JsonException)
            {
                return new AppSettings();
            }
            catch (IOException)
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsPath, json);
        }
    }
}