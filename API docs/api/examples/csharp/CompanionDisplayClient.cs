using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MadoMX
{
    /// <summary>
    /// Client for sending plugin state to the Mado MX display.
    /// </summary>
    public class CompanionDisplayClient : IDisposable
    {
        private readonly string _pluginId;
        private readonly string _displayName;
        private readonly string _stateDir;
        private readonly string _filePath;
        private bool _disposed;

        /// <summary>
        /// Creates a new CompanionDisplayClient.
        /// </summary>
        /// <param name="pluginId">Unique plugin identifier (lowercase, no spaces)</param>
        /// <param name="displayName">Human-readable name shown on display</param>
        public CompanionDisplayClient(string pluginId, string displayName)
        {
            _pluginId = pluginId ?? throw new ArgumentNullException(nameof(pluginId));
            _displayName = displayName ?? throw new ArgumentNullException(nameof(displayName));

            _stateDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MadoMX",
                "plugins"
            );

            _filePath = Path.Combine(_stateDir, $"{_pluginId}.json");

            // Ensure directory exists
            Directory.CreateDirectory(_stateDir);
        }

        /// <summary>
        /// Updates the display with the specified widgets.
        /// </summary>
        public void UpdateWidgets(params Widget[] widgets)
        {
            UpdateWidgets((IEnumerable<Widget>)widgets);
        }

        /// <summary>
        /// Updates the display with the specified widgets.
        /// </summary>
        public void UpdateWidgets(IEnumerable<Widget> widgets)
        {
            var state = new PluginState
            {
                PluginId = _pluginId,
                DisplayName = _displayName,
                Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Widgets = new List<Widget>(widgets)
            };

            WriteState(state);
        }

        /// <summary>
        /// Clears this plugin's display.
        /// </summary>
        public void Clear()
        {
            UpdateWidgets(Array.Empty<Widget>());
        }

        /// <summary>
        /// Removes this plugin's state file.
        /// </summary>
        public void Remove()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
            }
            catch (IOException)
            {
                // Ignore errors during removal
            }
        }

        private void WriteState(PluginState state)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(state, options);

            // Atomic write: write to temp file, then rename
            var tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _filePath, overwrite: true);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Remove();
                _disposed = true;
            }
        }
    }

    #region State Models

    public class PluginState
    {
        public string PluginId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public long Updated { get; set; }
        public int Priority { get; set; }
        public List<Widget> Widgets { get; set; } = new();
    }

    [JsonDerivedType(typeof(TextWidget), "text")]
    [JsonDerivedType(typeof(ProgressWidget), "progress")]
    [JsonDerivedType(typeof(GridWidget), "grid")]
    [JsonDerivedType(typeof(ListWidget), "list")]
    public abstract class Widget
    {
        [JsonPropertyName("type")]
        public abstract string Type { get; }

        public string? Zone { get; set; }
        public string? Label { get; set; }
    }

    public class TextWidget : Widget
    {
        public override string Type => "text";
        public string Value { get; set; } = "";
        public string? Color { get; set; }

        public TextWidget() { }

        public TextWidget(string label, string value, string? zone = null)
        {
            Label = label;
            Value = value;
            Zone = zone;
        }
    }

    public class ProgressWidget : Widget
    {
        public override string Type => "progress";
        public double Value { get; set; }
        public double Max { get; set; } = 100;
        public string? Color { get; set; }

        public ProgressWidget() { }

        public ProgressWidget(string label, double value, double max = 100, string? zone = null)
        {
            Label = label;
            Value = value;
            Max = max;
            Zone = zone;
        }
    }

    public class GridWidget : Widget
    {
        public override string Type => "grid";
        public List<string> Items { get; set; } = new();
        public int? Selected { get; set; }

        public GridWidget() { }

        public GridWidget(IEnumerable<string> items, int? selected = null, string? zone = null)
        {
            Items = new List<string>(items);
            Selected = selected;
            Zone = zone ?? "bottom";
        }
    }

    public class ListWidget : Widget
    {
        public override string Type => "list";
        public List<ListItem> Items { get; set; } = new();

        public ListWidget() { }

        public ListWidget(IEnumerable<ListItem> items, string? zone = null)
        {
            Items = new List<ListItem>(items);
            Zone = zone;
        }
    }

    public class ListItem
    {
        public string Label { get; set; } = "";
        public string? Value { get; set; }
        public bool? Selected { get; set; }

        public ListItem() { }

        public ListItem(string label, string? value = null, bool selected = false)
        {
            Label = label;
            Value = value;
            Selected = selected ? true : null;
        }
    }

    #endregion

    #region Example Extensions

    /// <summary>
    /// Helper extensions for common plugin scenarios.
    /// </summary>
    public static class DisplayExtensions
    {
        /// <summary>
        /// Updates the display with a simple status and progress.
        /// </summary>
        public static void UpdateStatusDisplay(
            this CompanionDisplayClient client,
            string statusLabel,
            string statusValue,
            string progressLabel,
            int progressValue)
        {
            client.UpdateWidgets(
                new TextWidget(statusLabel, statusValue, "main"),
                new ProgressWidget(progressLabel, progressValue, 100, "main")
            );
        }
    }

    #endregion
}
