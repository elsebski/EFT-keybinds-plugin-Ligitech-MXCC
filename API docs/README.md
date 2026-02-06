# MX Companion Display - Plugin API

Build plugins that display real-time information on your ESP32-CYD companion display alongside your Logitech MX Creative Console.

## Overview

The MX Companion Display uses a **file-based communication system**:

1. Your plugin writes a JSON file to a watched directory
2. The Bridge monitors for changes and forwards data to the ESP32
3. The ESP32 renders your widgets on the 320x240 display
4. Touch events are sent back to your plugin via a JSON file

This approach is simple, language-agnostic, and requires no network setup.

## Quick Start

### 1. Find the Plugin Directory

| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\MXCompanionDisplay\plugins\` |
| macOS | `~/.config/MXCompanionDisplay/plugins/` |

### 2. Create a Plugin JSON File

Create `myplugin.json` in the plugins directory:

```json
{
  "pluginId": "myplugin",
  "displayName": "My Plugin",
  "widgets": [
    {"type": "text", "label": "Status", "value": "Running", "color": "green"}
  ]
}
```

### 3. Update When State Changes

Overwrite the JSON file whenever your plugin's state changes. The bridge detects changes automatically.

---

## Companion App Integration

If you're building a plugin that integrates with Logi Options+, the companion app automatically discovers plugins from the Logi plugins directory.

### Plugin Location

| Platform | Path |
|----------|------|
| Windows | `%LOCALAPPDATA%\Logi\LogiPluginService\Plugins\` |
| macOS | `~/Library/Application Support/Logi/LogiPluginService/Plugins/` |

### Required Folder Structure

Your Logi plugin must include a `companion/` subfolder:

```
YourPlugin/
├── companion/
│   ├── manifest.yaml      # Plugin metadata (required)
│   ├── daemon.py          # Background script (optional)
│   └── yourplugin.json    # Widget data written by daemon
├── plugin.json            # Logi Options+ plugin manifest
└── ... (other Logi plugin files)
```

### manifest.yaml Format

```yaml
pluginId: yourplugin
displayName: Your Plugin
version: 1.0.0
description: Brief description of what the plugin does
author: Your Name
autoStart: true
entryPoint: daemon.py
```

| Field | Required | Default | Description |
|-------|----------|---------|-------------|
| `pluginId` | Yes | - | Unique identifier (lowercase, no spaces) |
| `displayName` | Yes | - | Name shown in companion app UI |
| `version` | No | `1.0.0` | Semantic version |
| `description` | No | - | Brief description |
| `author` | No | - | Plugin author |
| `autoStart` | No | `true` | Start daemon automatically on app launch |
| `entryPoint` | No | `daemon.py` | Python script to run as daemon |

### Daemon Script

The daemon is a background Python script that:
1. Listens for input (hotkeys, events, etc.)
2. Performs actions (volume control, API calls, etc.)
3. Writes widget JSON to the `companion/` folder

The bridge watches for changes to `*.json` files in `companion/` folders and forwards them to the ESP32.

### Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│  Logi Options+ (triggers actions via keyboard shortcuts)    │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  Your Daemon (companion/daemon.py)                          │
│  - Intercepts hotkeys or events                             │
│  - Performs plugin logic                                    │
│  - Writes JSON to companion/yourplugin.json                 │
└─────────────────────┬───────────────────────────────────────┘
                      │ (file change detected by watchdog)
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  MX Companion Display Bridge                                │
│  - Reads JSON from companion/ folder                        │
│  - Sends PLUGIN:id:json to ESP32                            │
└─────────────────────┬───────────────────────────────────────┘
                      │ Serial (230400 baud)
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  ESP32-CYD Display                                          │
│  - Renders widgets on 320x240 screen                        │
└─────────────────────────────────────────────────────────────┘
```

### Example Daemon Script

```python
#!/usr/bin/env python3
"""Example companion daemon."""

import json
import time
from pathlib import Path

# Determine companion folder (same directory as this script)
COMPANION_DIR = Path(__file__).parent
PLUGIN_ID = "example"

def update_display(widgets: list):
    """Write widget data to JSON file."""
    data = {
        "pluginId": PLUGIN_ID,
        "displayName": "Example",
        "updated": int(time.time()),
        "widgets": widgets
    }

    plugin_file = COMPANION_DIR / f"{PLUGIN_ID}.json"
    temp_file = plugin_file.with_suffix(".tmp")

    # Atomic write
    with open(temp_file, "w") as f:
        json.dump(data, f)
    temp_file.replace(plugin_file)

def main():
    # Initial display
    update_display([
        {"type": "text", "label": "Status", "value": "Running", "color": "green"}
    ])

    # Your plugin logic here...
    while True:
        time.sleep(1)

if __name__ == "__main__":
    main()
```

### Standalone Plugins (No Logi Integration)

If your plugin doesn't need Logi Options+ integration, simply write JSON files directly to the standalone plugins directory:

| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\MXCompanionDisplay\plugins\` |
| macOS | `~/.config/MXCompanionDisplay/plugins/` |

These are watched separately and don't require a `manifest.yaml`.

---

## Plugin JSON Format

### Required Fields

| Field | Type | Description |
|-------|------|-------------|
| `pluginId` | string | Unique identifier (lowercase, no spaces) |
| `displayName` | string | Name shown in display header (max 16 chars) |
| `widgets` | array | List of widgets to display (max 5 total) |

### Optional Fields

| Field | Type | Description |
|-------|------|-------------|
| `version` | string | Plugin version |
| `author` | string | Plugin author |
| `updated` | number | Unix timestamp of last update |

---

## Display Layout

The display is divided into zones:

```
+----------------------------------+ 0
|        HEADER (32px)             |
|  [Profile Name]    [Plugin Name] |
+----------------------------------+ 32
|                                  |
|        MAIN ZONE (156px)         |
|    Text/Progress/List widgets    |
|                                  |
+----------------------------------+ 188
|        BOTTOM ZONE (52px)        |
|    [Btn1] [Btn2] [Btn3] [Btn4]   |
+----------------------------------+ 240
```

- **Header**: Shows Logi Options+ profile name (left) and plugin display name (right)
- **Main zone**: Primary content area for text, progress, and list widgets
- **Bottom zone**: Touch-enabled button bar (grid widget only)

---

## Widget Reference

### Text Widget

Displays a label and text value.

```json
{
  "type": "text",
  "label": "Current App",
  "value": "Discord",
  "color": "cyan"
}
```

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `type` | string | Yes | - | Must be `"text"` |
| `label` | string | Yes | - | Small gray label above value |
| `value` | string | Yes | - | Large colored value text |
| `color` | string | No | `"white"` | Value text color |

---

### Progress Widget

Displays a progress bar with percentage.

```json
{
  "type": "progress",
  "label": "Volume",
  "value": 75,
  "max": 100,
  "color": "green"
}
```

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `type` | string | Yes | - | Must be `"progress"` |
| `label` | string | Yes | - | Label with auto-calculated percentage |
| `value` | number | Yes | - | Current value |
| `max` | number | No | `100` | Maximum value |
| `color` | string | No | `"cyan"` | Progress bar color |

---

### List Widget

Displays a vertical list of selectable items. Touch-enabled.

```json
{
  "type": "list",
  "items": [
    {"label": "Discord", "value": "75%", "selected": true},
    {"label": "Spotify", "value": "50%"},
    {"label": "Chrome", "value": "100%"}
  ]
}
```

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `type` | string | Yes | - | Must be `"list"` |
| `items` | array | Yes | - | List items (max 5) |
| `items[].label` | string | Yes | - | Item label (left-aligned) |
| `items[].value` | string | No | `""` | Item value (right-aligned, cyan) |
| `items[].selected` | boolean | No | `false` | Highlight this item |

**Touch Event**: `TOUCH:pluginId:list:index`

---

### Grid Widget

Displays a horizontal row of touch buttons. **Must be in bottom zone.**

```json
{
  "type": "grid",
  "zone": "bottom",
  "items": ["Discord", "Spotify", "Chrome", "Teams"],
  "selected": 0
}
```

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `type` | string | Yes | - | Must be `"grid"` |
| `zone` | string | Yes | - | Must be `"bottom"` |
| `items` | array | Yes | - | Button labels (max 6) |
| `selected` | number | No | `-1` | Highlighted button index |

**Touch Event**: `TOUCH:pluginId:grid:index`

---

## Zones

Widgets can be placed in different zones using the `zone` property:

| Zone | Height | Supported Widgets |
|------|--------|-------------------|
| `main` | 156px | text, progress, list |
| `bottom` | 52px | grid only |

If `zone` is omitted, widgets default to the main zone.

**Widget Limits:**
- Main zone: Up to 4 widgets
- Bottom zone: 1 grid widget
- Total: 5 widgets maximum

---

## Colors

Named colors available for text and progress widgets:

| Name | Hex | Usage |
|------|-----|-------|
| `white` | #FFFFFF | Default for text |
| `gray` | #808080 | Subtle/disabled |
| `cyan` | #00FFFF | Default for progress, accents |
| `red` | #FF0000 | Errors, warnings |
| `green` | #00FF00 | Success, active |
| `yellow` | #FFFF00 | Caution |
| `orange` | #FFA500 | Warnings |
| `blue` | #0000FF | Info |
| `magenta` | #FF00FF | Special |

---

## Touch Events

When the user touches a list item or grid button, the ESP32 sends an event to the bridge.

### Event Format

```
TOUCH:{pluginId}:{widgetType}:{index}
```

### Reading Touch Events

The bridge writes touch events to a JSON file:

| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\MXCompanionDisplay\touch_events.json` |
| macOS | `~/.config/MXCompanionDisplay/touch_events.json` |

**File content:**
```json
{
  "pluginId": "appvolume",
  "widgetType": "grid",
  "index": 2,
  "timestamp": 1706900000.123
}
```

Watch this file for changes to respond to touch input.

---

## Serial Protocol

For direct ESP32 communication (advanced usage):

### Commands (PC → ESP32)

| Command | Format | Description |
|---------|--------|-------------|
| TEXT | `TEXT:message\n` | Display centered message |
| PROFILE | `PROFILE:name\n` | Set profile in header |
| PLUGIN | `PLUGIN:id:json\n` | Send plugin widget data |
| CLEAR | `CLEAR\n` | Clear display |
| PING | `PING\n` | Connection check |
| LED | `LED:r,g,b\n` | Set LED color (0-255 each) |
| LED | `LED:CYCLE\n` | Enable rainbow cycling |
| LED | `LED:OFF\n` | Turn off LED |

### Responses (ESP32 → PC)

| Response | Description |
|----------|-------------|
| `READY\n` | Device ready after boot |
| `OK:command\n` | Command successful |
| `ERR:reason\n` | Command failed |
| `PONG\n` | Ping response |
| `TOUCH:plugin:type:idx\n` | Touch event |
| `STANDBY\n` | Device entering standby |
| `WAKE\n` | Device woke from standby |

---

## C# Integration Example

```csharp
using System;
using System.IO;
using System.Text.Json;

public class CompanionDisplayClient
{
    private readonly string _pluginId;
    private readonly string _displayName;
    private readonly string _pluginsDir;
    private readonly string _touchEventsFile;

    public CompanionDisplayClient(string pluginId, string displayName)
    {
        _pluginId = pluginId;
        _displayName = displayName;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var baseDir = Path.Combine(appData, "MXCompanionDisplay");
        _pluginsDir = Path.Combine(baseDir, "plugins");
        _touchEventsFile = Path.Combine(baseDir, "touch_events.json");

        Directory.CreateDirectory(_pluginsDir);
    }

    public void UpdateDisplay(object[] widgets)
    {
        var state = new
        {
            pluginId = _pluginId,
            displayName = _displayName,
            updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            widgets = widgets
        };

        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var filePath = Path.Combine(_pluginsDir, $"{_pluginId}.json");
        var tempPath = filePath + ".tmp";

        // Atomic write
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, filePath, overwrite: true);
    }

    public TouchEvent? ReadTouchEvent()
    {
        if (!File.Exists(_touchEventsFile)) return null;

        try
        {
            var json = File.ReadAllText(_touchEventsFile);
            return JsonSerializer.Deserialize<TouchEvent>(json);
        }
        catch
        {
            return null;
        }
    }
}

public record TouchEvent(string PluginId, string WidgetType, int Index, double Timestamp);

// Usage Example
var display = new CompanionDisplayClient("myapp", "My App");

// Show volume control with app selector
display.UpdateDisplay(new object[]
{
    new { type = "text", label = "Output", value = "System", color = "white" },
    new { type = "progress", label = "Volume", value = 75, max = 100, color = "green" },
    new {
        type = "grid",
        zone = "bottom",
        items = new[] { "Discord", "Spotify", "Chrome" },
        selected = 0
    }
});
```

---

## Python Integration Example

```python
import json
import os
import sys
import time
from pathlib import Path
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

# Platform-specific paths
if sys.platform == "win32":
    BASE_DIR = Path(os.environ.get("APPDATA", "")) / "MXCompanionDisplay"
else:
    BASE_DIR = Path.home() / ".config" / "MXCompanionDisplay"

PLUGINS_DIR = BASE_DIR / "plugins"
TOUCH_EVENTS_FILE = BASE_DIR / "touch_events.json"


class CompanionDisplayClient:
    def __init__(self, plugin_id: str, display_name: str):
        self.plugin_id = plugin_id
        self.display_name = display_name
        PLUGINS_DIR.mkdir(parents=True, exist_ok=True)

    def update_display(self, widgets: list):
        """Update the display with new widget data."""
        data = {
            "pluginId": self.plugin_id,
            "displayName": self.display_name,
            "updated": int(time.time()),
            "widgets": widgets
        }

        plugin_file = PLUGINS_DIR / f"{self.plugin_id}.json"
        temp_file = plugin_file.with_suffix(".tmp")

        # Atomic write
        with open(temp_file, "w") as f:
            json.dump(data, f)
        temp_file.replace(plugin_file)

    def read_touch_event(self) -> dict | None:
        """Read the latest touch event."""
        if not TOUCH_EVENTS_FILE.exists():
            return None
        try:
            with open(TOUCH_EVENTS_FILE) as f:
                return json.load(f)
        except:
            return None


# Usage Example
display = CompanionDisplayClient("myapp", "My App")

display.update_display([
    {"type": "text", "label": "Status", "value": "Active", "color": "green"},
    {"type": "progress", "label": "CPU", "value": 45, "max": 100},
    {"type": "list", "items": [
        {"label": "Process 1", "value": "12%", "selected": True},
        {"label": "Process 2", "value": "8%"},
        {"label": "Process 3", "value": "5%"}
    ]}
])
```

---

## Audio Service (Windows Only)

The bridge includes a built-in Audio Service that provides real-time volume monitoring for plugins. Instead of plugins polling audio themselves, the bridge handles it at high frequency for smooth display updates.

### How It Works

1. Plugin writes `audio_selection.json` to specify which app to monitor
2. Bridge reads the selection and polls volume via pycaw at 30Hz
3. Bridge generates volume widgets automatically and sends directly to serial
4. Display updates in real-time as volume changes (no file I/O in hot path)

### audio_selection.json Format

Create `audio_selection.json` in your plugin's `companion/` folder:

```json
{
  "processName": "Discord",
  "displayName": "Discord"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `processName` | string | Yes | Process name (without .exe) to monitor |
| `displayName` | string | No | Display name shown on screen (defaults to processName) |

### Generated Widgets

When a plugin has an audio selection, the bridge automatically generates:

```json
[
  {"type": "text", "label": "App", "value": "Discord", "color": "cyan"},
  {"type": "progress", "label": "Volume", "value": 75, "max": 100, "color": "green"}
]
```

- **App widget**: Shows app name, gray if inactive
- **Volume widget**: Shows volume bar, red if muted with "(MUTED)" label

### Merging with Plugin Widgets

If your plugin also writes a regular `{pluginId}.json` with widgets (e.g., a grid for app selection), the bridge merges them:

- Audio widgets (text + progress) are added first
- Grid and list widgets from your JSON are appended

This lets you provide app selection buttons while the bridge handles volume display.

### Example: App Volume Plugin

**audio_selection.json** (written when user selects an app):
```json
{
  "processName": "Spotify",
  "displayName": "Spotify"
}
```

**appvolume.json** (for app selection grid):
```json
{
  "pluginId": "appvolume",
  "displayName": "Volume",
  "widgets": [
    {
      "type": "grid",
      "zone": "bottom",
      "items": ["Discord", "Spotify", "Chrome", "Teams"],
      "selected": 1
    }
  ]
}
```

**Result on display**:
- Text: "App: Spotify" (cyan)
- Progress: "Volume: 75%" (green bar)
- Grid: [Discord] [**Spotify**] [Chrome] [Teams]

### Removing Audio Selection

Delete `audio_selection.json` to stop audio monitoring for a plugin. The bridge will stop generating audio widgets and fall back to the plugin's regular JSON widgets.

---

## Real-time IPC (Named Pipe / Unix Socket)

For plugins that need to send data at high frequency (>10Hz) without the overhead of file I/O, the bridge provides a real-time IPC channel.

### How It Works

1. Bridge starts a named pipe (Windows) or Unix socket (macOS/Linux) server
2. Plugin connects and sends JSON lines
3. Bridge forwards data directly to the ESP32 serial port
4. No file system involved - minimal latency

### Connection Details

| Platform | Address |
|----------|---------|
| Windows | `\\.\pipe\MXCompanionDisplay` |
| macOS/Linux | `~/.config/MXCompanionDisplay/realtime.sock` |

### Protocol

Send newline-delimited JSON messages:

```json
{"pluginId": "myplugin", "displayName": "My Plugin", "widgets": [...]}
```

Each message is a complete plugin data object, same format as the file-based API.

### Python Example

```python
import json
import socket
import sys
from pathlib import Path

def send_to_display(data: dict):
    """Send plugin data via real-time IPC."""
    message = json.dumps(data) + "\n"

    if sys.platform == "win32":
        # Windows named pipe
        import win32file
        pipe = win32file.CreateFile(
            r"\\.\pipe\MXCompanionDisplay",
            win32file.GENERIC_WRITE,
            0, None,
            win32file.OPEN_EXISTING,
            0, None
        )
        win32file.WriteFile(pipe, message.encode("utf-8"))
        win32file.CloseHandle(pipe)
    else:
        # Unix socket
        sock_path = Path.home() / ".config" / "MXCompanionDisplay" / "realtime.sock"
        with socket.socket(socket.AF_UNIX, socket.SOCK_STREAM) as sock:
            sock.connect(str(sock_path))
            sock.sendall(message.encode("utf-8"))


# Usage - send at any frequency (30Hz, 60Hz, etc.)
send_to_display({
    "pluginId": "myaudio",
    "displayName": "Audio",
    "widgets": [
        {"type": "progress", "label": "Volume", "value": 75, "max": 100}
    ]
})
```

### C# Example (Windows)

```csharp
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

public static void SendToDisplay(object data)
{
    var json = JsonSerializer.Serialize(data) + "\n";
    var bytes = Encoding.UTF8.GetBytes(json);

    using var pipe = new NamedPipeClientStream(".", "MXCompanionDisplay", PipeDirection.Out);
    pipe.Connect(1000);  // 1 second timeout
    pipe.Write(bytes, 0, bytes.Length);
}
```

### When to Use IPC vs File-Based

| Use Case | Recommended Approach |
|----------|---------------------|
| Updates < 10Hz | File-based (simpler) |
| Updates 10-60Hz | IPC (real-time pipe) |
| Audio meters/visualizers | IPC |
| Static info/status | File-based |
| Cross-language plugins | File-based (universal) |

### Persistent Connection

For high-frequency updates, you can keep the connection open:

```python
import json
import socket
from pathlib import Path

class RealtimeClient:
    def __init__(self):
        self._sock = None

    def connect(self):
        sock_path = Path.home() / ".config" / "MXCompanionDisplay" / "realtime.sock"
        self._sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        self._sock.connect(str(sock_path))

    def send(self, data: dict):
        if self._sock:
            message = json.dumps(data) + "\n"
            self._sock.sendall(message.encode("utf-8"))

    def close(self):
        if self._sock:
            self._sock.close()
            self._sock = None


# Keep connection open for high-frequency sends
client = RealtimeClient()
client.connect()

# Send updates as fast as needed
for volume in range(0, 100):
    client.send({"pluginId": "meter", "displayName": "VU", "widgets": [
        {"type": "progress", "label": "Level", "value": volume, "max": 100}
    ]})
    time.sleep(0.016)  # ~60Hz

client.close()
```

---

## Best Practices

### Update Frequency
- Don't update faster than 10 times per second
- Batch multiple changes into single updates
- Use `updated` timestamp to prevent redundant writes

### File Atomicity
- Write to a `.tmp` file, then rename/move to the final path
- This prevents partial reads by the bridge

### Plugin ID
- Use lowercase letters, numbers, hyphens, and underscores only
- Keep it short and unique (e.g., `appvolume`, `system-stats`)

### Display Name
- Keep under 16 characters
- This appears in the header on the right side

### Touch Handling
- Poll the touch events file or watch for changes
- Check the `timestamp` to avoid processing stale events
- Clear/delete the file after processing if needed

---

## Troubleshooting

### Plugin Not Showing

1. Verify JSON is valid (use a JSON validator)
2. Check file is in the correct plugins directory
3. Ensure the bridge is running and connected
4. Check `pluginId` matches filename (without `.json`)

### Display Not Updating

1. Update the `updated` timestamp with each change
2. Check for JSON errors in bridge console
3. Try the `CLEAR` command to reset

### Touch Not Working

1. Verify widgets are in touchable zones (grid in bottom, list in main)
2. Check the touch events file is being written
3. Touch requires firm press on CYD screen

### Connection Issues

1. Ensure only one application uses the serial port
2. Close the bridge before flashing new ESP32 firmware
3. Try reconnecting via the config app
