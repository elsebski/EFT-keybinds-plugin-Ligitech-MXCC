using System;
using System.IO;
using System.Text.Json;

namespace Loupedeck.TarkovKeybindPlugin
{
    public class CompanionDisplayClient
    {
        private readonly string _pluginId;
        private readonly string _displayName;
        private readonly string _pluginsDir;
        private readonly string _filePath;
        public CompanionDisplayClient(string pluginId, string displayName)
        {
            _pluginId = pluginId;
            _displayName = displayName;

            try
            {
                // Write into the Logi plugin's companion/ folder - this is where
                // the Mado MX bridge watches for plugin JSON files.
                // Path: %LOCALAPPDATA%\Logi\LogiPluginService\Plugins\TarkovKeybindPlugin\companion\
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                _pluginsDir = Path.Combine(localAppData, "Logi", "LogiPluginService", "Plugins", "TarkovKeybindPlugin", "companion");
                _filePath = Path.Combine(_pluginsDir, $"{_pluginId}.json");

                if (!Directory.Exists(_pluginsDir))
                {
                    Directory.CreateDirectory(_pluginsDir);
                }

                PluginLog.Write($"Initialized Mado MX client. Plugins dir: {_pluginsDir}");
            }
            catch (Exception ex)
            {
                PluginLog.Write($"Failed to initialize companion client: {ex.Message}");
            }
        }

        public void UpdateDisplay(string label, string value)
        {
            try
            {
                // Reload settings each time so changes from Mado MX UI apply immediately
                var config = DisplayConfig.Load();

                var state = new
                {
                    pluginId = _pluginId,
                    displayName = _displayName,
                    updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    widgets = new object[]
                    {
                        new
                        {
                            type = "textrows",
                            rows = new object[]
                            {
                                new
                                {
                                    label = label,
                                    value = value,
                                    color = config.Color,
                                    labelFontSize = config.LabelFontSize,
                                    valueFontSize = config.ValueFontSize,
                                    labelAlign = config.LabelAlign,
                                    valueAlign = config.ValueAlign
                                }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Atomic write: write to temp file, then rename
                var tempPath = _filePath + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _filePath, overwrite: true);
            }
            catch (Exception ex)
            {
                PluginLog.Write($"Failed to update companion display: {ex.Message}");
            }
        }
    }
}
