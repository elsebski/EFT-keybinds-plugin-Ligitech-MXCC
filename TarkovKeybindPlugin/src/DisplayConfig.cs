using System;
using System.IO;
using System.Text.Json;

namespace Loupedeck.TarkovKeybindPlugin
{
    /// <summary>
    /// Reads display settings from Mado MX's plugin_settings.json.
    /// Settings are configured via the Mado MX companion app UI (settingsSchema in manifest.yaml).
    /// File location: %APPDATA%\MadoMX\plugin_settings.json
    /// </summary>
    public class DisplayConfig
    {
        public string Color { get; set; } = "orange";
        public string LabelFontSize { get; set; } = "small";
        public string ValueFontSize { get; set; } = "xlarge";
        public string LabelAlign { get; set; } = "center";
        public string ValueAlign { get; set; } = "center";

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MadoMX", "plugin_settings.json");

        public static DisplayConfig Load()
        {
            var config = new DisplayConfig();

            try
            {
                if (!File.Exists(SettingsPath))
                    return config;

                var json = File.ReadAllText(SettingsPath);
                using var doc = JsonDocument.Parse(json);

                // Structure: { "tarkovkeybinds": { "settings": { "color": "orange", ... } } }
                if (doc.RootElement.TryGetProperty("tarkovkeybinds", out var pluginEl) &&
                    pluginEl.TryGetProperty("settings", out var settingsEl))
                {
                    if (settingsEl.TryGetProperty("color", out var color))
                        config.Color = color.GetString() ?? config.Color;

                    if (settingsEl.TryGetProperty("labelFontSize", out var lfs))
                        config.LabelFontSize = lfs.GetString() ?? config.LabelFontSize;

                    if (settingsEl.TryGetProperty("valueFontSize", out var vfs))
                        config.ValueFontSize = vfs.GetString() ?? config.ValueFontSize;

                    if (settingsEl.TryGetProperty("labelAlign", out var la))
                        config.LabelAlign = la.GetString() ?? config.LabelAlign;

                    if (settingsEl.TryGetProperty("valueAlign", out var va))
                        config.ValueAlign = va.GetString() ?? config.ValueAlign;

                    PluginLog.Write($"Loaded Mado MX settings: color={config.Color}, valueFont={config.ValueFontSize}, labelFont={config.LabelFontSize}");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Write($"Error reading Mado MX settings (using defaults): {ex.Message}");
            }

            return config;
        }
    }
}
