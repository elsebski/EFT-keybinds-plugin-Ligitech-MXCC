# Mado MX - Plugin API

Build plugins that display real-time information on your ESP32-CYD companion display alongside your Logitech MX Creative Console.

## Overview

The Mado MX uses a **file-based communication system**:

1. Your plugin writes a JSON file to a watched directory
2. The Bridge monitors for changes and forwards data to the ESP32
3. The ESP32 renders your widgets on the 320x240 display
4. Touch events are sent back to your plugin via a JSON file

This approach is simple, language-agnostic, and requires no network setup.

## Table of Contents

### Getting Started
- [Quick Start](#quick-start) - Create your first plugin in minutes
- [Plugin Development Guide](#plugin-development-guide) - Complete step-by-step tutorial

### Plugin Types & Integration
- [Companion App Integration](#companion-app-integration) - Auto-starting daemons with Logi Options+
- [Installing Plugins via Companion App](#installing-plugins-via-companion-app) - User installation workflow
- [Standalone Plugins](#standalone-plugins-no-logi-integration) - Simple file-based plugins

### Plugin Configuration
- [Plugin JSON Format](#plugin-json-format) - Widget data structure
- [Plugin Settings UI](#plugin-settings-ui-companion-app) - Add custom controls to companion app
- [manifest.yaml Format](#manifestyaml-format) - Plugin metadata specification

### Display & Widgets
- [Display Layout](#display-layout) - Screen zones and dimensions
- [Widget Reference](#widget-reference) - All widget types with examples
  - [Text Widget](#text-widget)
  - [Progress Widget](#progress-widget)
  - [Icon Widget](#icon-widget)
  - [TextRows Widget](#textrows-widget)
  - [List Widget](#list-widget)
  - [Grid Widget](#grid-widget)
- [Colors](#colors) - Available color palette

### Advanced Features
- [Touch Events](#touch-events) - Handle user interactions
- [Audio Service](#audio-service-windows-only) - Built-in volume monitoring
- [Real-time IPC](#real-time-ipc-named-pipe--unix-socket) - High-frequency updates (>10Hz)
- [Serial Protocol](#serial-protocol) - Direct ESP32 communication

### Integration Examples
- [C# Integration](#c-integration-example)
- [Python Integration](#python-integration-example)

### Reference
- [Best Practices](#best-practices) - Tips for robust plugins
- [Troubleshooting](#troubleshooting) - Common issues and solutions

## Quick Start

### Option A: Standalone Plugin (No Logi Integration)

For simple plugins that don't need Logi Options+ console controls:

1. **Find the standalone plugin directory:**

   | Platform | Path |
   |----------|------|
   | Windows | `%APPDATA%\MadoMX\plugins\` |
   | macOS | `~/.config/MadoMX/plugins/` |

2. **Create a plugin JSON file** (`myplugin.json`):

   ```json
   {
     "pluginId": "myplugin",
     "displayName": "My Plugin",
     "widgets": [
       {"type": "text", "label": "Status", "value": "Running", "color": "green"}
     ]
   }
   ```

3. **Update when state changes**: Overwrite the JSON file whenever your plugin's state changes. The bridge detects changes automatically.

### Option B: Full Plugin (With Logi Integration)

For plugins that integrate with Logi Options+ console controls:

1. **Create plugin folder structure** (see "Companion App Integration" below)
2. **Install via companion app** using the "Add Plugin" button
3. **Restart Logi Options+** to load the plugin
4. **Daemon auto-starts** and writes widget data to `companion/*.json`

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

### Plugin Icons

Add a custom icon to your plugin to appear in the companion app UI:

1. Create a 256x256 PNG icon
2. Save it as `metadata/Icon256x256.png` in your plugin folder:
   ```
   YourPlugin/
   ├── metadata/
   │   └── Icon256x256.png      # 256x256 PNG
   └── companion/
       └── manifest.yaml
   ```

3. The icon will appear:
   - In the Plugins list
   - On the plugin detail page
   - In the sidebar submenu

If no icon is provided, a default plugin icon (puzzle piece) is used.

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
│  Mado MX Bridge                                │
│  - Reads JSON from companion/ folder                        │
│  - Sends PLUGIN:id:json to ESP32                            │
└─────────────────────┬───────────────────────────────────────┘
                      │ Serial (460800 baud)
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
| Windows | `%APPDATA%\MadoMX\plugins\` |
| macOS | `~/.config/MadoMX/plugins/` |

These are watched separately and don't require a `manifest.yaml`.

### Auto-Discovery and Plugin List

The companion app automatically discovers plugins in the following ways:

**Companion Plugins (from Logi directory):**
- Scans `%LOCALAPPDATA%\Logi\LogiPluginService\Plugins\` on startup
- Looks for folders containing `companion/manifest.yaml`
- Plugins appear in the sidebar and Plugins list immediately
- Daemon auto-starts if `autoStart: true` in manifest

**Standalone Plugins:**
- Bridge watches `%APPDATA%\MadoMX\plugins\` for `*.json` files
- **Note**: Standalone plugins do NOT appear in the companion app's Plugins list
- They are forwarded directly to the display via the bridge
- No settings UI or daemon management for standalone plugins

**When plugins appear in the UI:**
- Companion plugins: Always visible after installation (have metadata from manifest.yaml)
- Standalone plugins: Not shown in UI (data-only, no management interface)

**Refreshing the plugin list:**
- Automatically rescanned when navigating to the Plugins section
- Restart companion app to detect newly installed plugins
- Daemon status is checked in real-time

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
| `duration_ms` | number | Display duration in milliseconds (0 or absent = uses global default, typically 2000ms) |

### Duration-Based Display with Idle Animations

Plugins can specify how long they should be displayed before automatically returning to an idle animation:

```json
{
  "pluginId": "notification",
  "displayName": "Alert",
  "duration_ms": 5000,
  "widgets": [...]
}
```

**Behavior:**
- **Persistent plugins** (`duration_ms: 0`): Stay on screen until another plugin overwrites them
- **Timed plugins** (`duration_ms > 0`): Display for the specified duration, then return to idle animation
- **Default duration**: If `duration_ms` is omitted, the global default from companion app settings is used (typically 2000ms)
- **Last to send wins**: The newest plugin data always displays immediately, regardless of timing

**Idle Animations:**

When a timed plugin expires, the display returns to the selected idle animation:
- **Blank** - Black screen
- **Matrix Rain** - Falling green code effect (Matrix-style)
- **Rainbow Plasma** - Morphing multi-wave color plasma
- **Lava Lamp** - Floating metaball blobs with smooth color blending

Users configure the idle animation in the companion app Settings → Display section.

**Use cases:**
- **Persistent** (`duration_ms: 0`): Clock displays, system monitoring that should always be visible
- **Timed** (`duration_ms: 2000-5000`): Volume controls, notifications, alerts, status updates
- **Omit field**: Use global default (configurable by user, typically 2s)

**Example: Notification popup that returns to idle animation**
```json
{
  "pluginId": "alert",
  "displayName": "Alert",
  "duration_ms": 3000,
  "widgets": [
    {"type": "text", "label": "Status", "value": "New Message", "color": "yellow"}
  ]
}
```

After 3 seconds, the display automatically returns to the idle animation (Matrix Rain, Rainbow Plasma, Lava Lamp, or Blank).

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

### Icon Widget

Displays an icon with optional label and value. Icons are 32x32 monochrome bitmaps rendered at runtime in any color.

```json
{
  "type": "icon",
  "icon": "speaker",
  "state": "idle",
  "label": "Volume",
  "value": "75%",
  "color": "cyan"
}
```

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `type` | string | Yes | - | Must be `"icon"` |
| `icon` | string | Yes | - | Icon name (see Available Icons below) |
| `state` | string | No | `"idle"` | Animation state (see States below) |
| `label` | string | No | `""` | Small gray label below icon |
| `value` | string | No | `""` | Large colored value below label |
| `color` | string | No | `"white"` | Icon and value color |

**Available Icons:**
- `speaker` - Speaker with sound waves
- `speaker-mute` - Speaker with X (muted)
- `mic` - Microphone
- `mic-mute` - Microphone with slash (muted)
- `bell` - Notification bell
- `check` - Checkmark (success)
- `warning` - Triangle with exclamation mark
- `cpu` - CPU chip
- `battery` - Battery indicator
- `network` - WiFi signal

**States** (Phase 1: only `idle` is implemented):
- `idle` - Static display
- `active` - Pulsing animation (planned)
- `alert` - Fast pulse animation (planned)
- `wave` - Expanding circles (planned)
- `blink` - Blinking visibility (planned)
- `progress` - Circular progress arc (planned)

**Use Cases:**
- Audio status (speaker/mic icons with mute states)
- System monitoring (cpu, battery, network)
- Notifications (bell, check, warning)
- App status indicators

---

### TextRows Widget

Displays 1-3 label/value pairs that fill the entire screen below the header. Each row has a small label and large value, with independently configurable font sizes, colors, and alignments. Perfect for full-screen data displays like clocks, volume controls, or status dashboards.

```json
{
  "type": "textrows",
  "rows": [
    {
      "label": "Volume",
      "value": "75%",
      "color": "cyan",
      "labelFontSize": "small",
      "labelAlign": "left",
      "valueFontSize": "xlarge",
      "valueAlign": "center"
    },
    {
      "label": "Mode",
      "value": "Normal",
      "color": "green",
      "labelFontSize": "small",
      "labelAlign": "left",
      "valueFontSize": "large",
      "valueAlign": "right"
    }
  ]
}
```

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `type` | string | Yes | - | Must be `"textrows"` |
| `rows` | array | Yes | - | Array of row objects (1-3 rows) |
| `rows[].label` | string | No | `""` | Small label text above value |
| `rows[].value` | string | Yes | - | Large value text |
| `rows[].color` | string | No | `"white"` | Value text color (see Colors section) |
| `rows[].labelFontSize` | string | No | `"small"` | Label font: `"tiny"`, `"small"`, `"medium"` |
| `rows[].labelAlign` | string | No | `"left"` | Label alignment: `"left"`, `"center"`, `"right"` |
| `rows[].valueFontSize` | string | No | `"large"` | Value font (see Font Sizes below) |
| `rows[].valueAlign` | string | No | `"left"` | Value alignment: `"left"`, `"center"`, `"right"` |

**Font Sizes for Values:**
- `"tiny"`: 8px height (TFT_eSPI Font 1)
- `"small"`: 16px height (Font 2)
- `"medium"`: 26px height (Font 4)
- `"large"`: 48px height (Font 6)
- `"xlarge"`: 75px height (Font 8 - proportional, best for text)
- `"xlarge-lcd"`: 48px height (Font 7 - 7-segment LCD style, **numbers only**)

**Layout Rules:**
- **1 row**: Label gets 28px, value gets 170px (supports up to `xlarge`)
- **2 rows**: Each label gets 28px, each value gets 71px (supports up to `xlarge` but tight)
- **3 rows**: Each label gets 28px, each value gets 38px (max `medium` for values)

**Font 7 (xlarge-lcd) Warning:**
Font 7 is a 7-segment LCD display font (like a digital clock) that **only renders numbers and basic symbols** well. Use it for numeric displays like "12:34", "75%", "3.14", etc. For text with letters, use `"xlarge"` instead.

**Use Cases:**
- **Full-screen clocks**: Huge time display with date below
- **Volume controls**: App name with large percentage
- **System monitoring**: CPU/RAM/Temp with big numbers
- **Status dashboards**: Multi-metric displays
- **Notifications**: Large, centered messages

**Example: Digital Clock (1 row with LCD font)**
```json
{
  "type": "textrows",
  "rows": [
    {
      "label": "Tuesday, Feb 6",
      "value": "12:34",
      "color": "white",
      "labelFontSize": "medium",
      "labelAlign": "center",
      "valueFontSize": "xlarge-lcd",
      "valueAlign": "center"
    }
  ]
}
```

**Example: Volume Display (2 rows)**
```json
{
  "type": "textrows",
  "rows": [
    {
      "label": "Output Device",
      "value": "Speakers",
      "color": "cyan",
      "labelFontSize": "small",
      "labelAlign": "left",
      "valueFontSize": "large",
      "valueAlign": "center"
    },
    {
      "label": "Volume",
      "value": "75%",
      "color": "green",
      "labelFontSize": "small",
      "labelAlign": "left",
      "valueFontSize": "xlarge",
      "valueAlign": "right"
    }
  ]
}
```

**Example: System Monitor (3 rows)**
```json
{
  "type": "textrows",
  "rows": [
    {
      "label": "CPU",
      "value": "45%",
      "color": "green",
      "labelFontSize": "small",
      "valueFontSize": "medium",
      "valueAlign": "right"
    },
    {
      "label": "Memory",
      "value": "8.2 GB",
      "color": "yellow",
      "labelFontSize": "small",
      "valueFontSize": "medium",
      "valueAlign": "right"
    },
    {
      "label": "Temp",
      "value": "62°C",
      "color": "cyan",
      "labelFontSize": "small",
      "valueFontSize": "medium",
      "valueAlign": "right"
    }
  ]
}
```

**Design Tips:**
- **Center alignment** works great for single-row displays (clocks, notifications)
- **Right alignment** is perfect for numbers and percentages (volume, stats)
- **Left alignment** is best for text labels and app names
- Use **xlarge-lcd** for retro digital clock aesthetics (numbers only!)
- Mix alignments: left labels with centered values looks professional

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
| Windows | `%APPDATA%\MadoMX\touch_events.json` |
| macOS | `~/.config/MadoMX/touch_events.json` |

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
| PLUGIN | `PLUGIN:id:json\n` | Send plugin widget data (JSON includes duration_ms) |
| CLEAR | `CLEAR\n` | Clear display |
| PING | `PING\n` | Connection check |
| IDLE | `IDLE:animation\n` | Set idle animation (`blank`, `matrix`, `rainbow`, `lava`) |
| PROGRESS | `PROGRESS:id:val:max:color:label\n` | Fast progress update (no JSON) |
| PEAK | `PEAK:id:value\n` | Ultra-fast peak level update (fire-and-forget) |
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

## Idle Animations

The display features three idle animations that appear when no plugin is active or after a timed plugin expires.

### Available Animations

**Blank** - Black screen
- Simple, low-power idle state
- Good for minimalist setups

**Matrix Rain** - Falling green code
- Matrix-style digital rain effect
- 53 columns of random characters
- Gradient colors from white (head) to dim green (tail)
- Runs at ~33 FPS

**Rainbow Plasma** - Morphing color plasma
- Four overlapping sine waves (horizontal, vertical, diagonal, radial)
- Creates organic, evolving color blobs across the full display
- Uses precomputed sine lookup table for performance
- Renders in 4x4 pixel blocks at ~30 FPS

**Lava Lamp** - Floating metaballs
- 5 colored blobs that drift, bounce, and merge smoothly
- Metaball field function creates soft edges where blobs overlap
- Colors blend organically with a slow global hue shift
- Blobs have independent velocities with sine-wave acceleration
- Renders in 4x4 pixel blocks at ~25 FPS

### Configuration

Users configure the idle animation in the companion app:
1. Open **Settings** → **Display**
2. Select **Idle Animation** from dropdown
3. Click **Save Settings**
4. The animation changes immediately (no restart needed)

### Default Duration

The companion app also provides a **Default Plugin Duration** setting (typically 2000ms). This applies to plugins that don't specify `duration_ms` in their JSON:

- User sets default to 3000ms
- Plugin sends data without `duration_ms` field
- Display shows plugin for 3 seconds
- Returns to idle animation

### ESP32 Timer Behavior

The ESP32 handles all duration timing autonomously:
- Plugin data received → display immediately, start timer
- Timer expires → clear display, show idle animation
- Bridge disconnects mid-timer → timer continues, expires normally
- No bridge-side timer needed

### Programming Considerations

**For persistent plugins** (clocks, system monitors):
```json
{
  "pluginId": "clock",
  "displayName": "Clock",
  "duration_ms": 0,
  "widgets": [...]
}
```

**For timed notifications** (use explicit duration):
```json
{
  "pluginId": "notification",
  "displayName": "Alert",
  "duration_ms": 5000,
  "widgets": [...]
}
```

**For standard plugins** (use global default):
```json
{
  "pluginId": "volume",
  "displayName": "Volume",
  "widgets": [...]
}
```
(Bridge applies user's default duration, typically 2000ms)

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
        var baseDir = Path.Combine(appData, "MadoMX");
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

// Example 1: Clock with TextRows
display.UpdateDisplay(new object[]
{
    new {
        type = "textrows",
        rows = new[] {
            new {
                label = "Tuesday, Feb 6",
                value = "12:34",
                color = "white",
                labelFontSize = "medium",
                labelAlign = "center",
                valueFontSize = "xlarge-lcd",
                valueAlign = "center"
            }
        }
    }
});

// Example 2: System monitoring
display.UpdateDisplay(new object[]
{
    new { type = "icon", icon = "cpu", label = "CPU", value = "45%", color = "cyan" },
    new { type = "icon", icon = "battery", label = "Battery", value = "87%", color = "green" },
    new { type = "icon", icon = "network", label = "Network", value = "Online", color = "green" }
});

// Example 3: Volume control with app selector
display.UpdateDisplay(new object[]
{
    new { type = "icon", icon = "speaker", label = "Output", value = "System", color = "cyan" },
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
    BASE_DIR = Path(os.environ.get("APPDATA", "")) / "MadoMX"
else:
    BASE_DIR = Path.home() / ".config" / "MadoMX"

PLUGINS_DIR = BASE_DIR / "plugins"
TOUCH_EVENTS_FILE = BASE_DIR / "touch_events.json"


class CompanionDisplayClient:
    def __init__(self, plugin_id: str, display_name: str):
        self.plugin_id = plugin_id
        self.display_name = display_name
        PLUGINS_DIR.mkdir(parents=True, exist_ok=True)

    def update_display(self, widgets: list, duration_ms: int = None):
        """Update the display with new widget data.

        Args:
            widgets: List of widget objects
            duration_ms: Display duration in milliseconds
                        None = use plugin/global default (recommended)
                        0 = persistent (always shown)
                        >0 = show for specified time then return to idle

        Best Practices:
            - For trigger-based plugins: Omit duration_ms (use None) to respect user settings
            - For persistent monitors: Use duration_ms=0
            - For temporary alerts: Use duration_ms=3000-5000
        """
        data = {
            "pluginId": self.plugin_id,
            "displayName": self.display_name,
            "updated": int(time.time()),
            "widgets": widgets
        }

        # Only include duration_ms if explicitly set
        # If None, bridge will apply plugin settings or global default
        if duration_ms is not None:
            data["duration_ms"] = duration_ms

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


# Usage Examples
display = CompanionDisplayClient("myapp", "My App")

# Example 1: Trigger-based plugin (volume control)
# Only call this when user spins dial - NOT in a loop!
def on_volume_change(volume):
    display.update_display([
        {"type": "progress", "label": "Volume", "value": volume, "max": 100, "color": "green"}
    ])
    # duration_ms omitted - uses plugin settings or global default
    # Will show for configured duration, then return to idle

# Example 2: Persistent monitor (clock - always visible)
# This one can loop because duration_ms=0 means never expire
def clock_loop():
    while True:
        now = datetime.now()
        display.update_display([
            {
                "type": "textrows",
                "rows": [{
                    "label": now.strftime("%A, %b %d"),
                    "value": now.strftime("%H:%M"),
                    "color": "white",
                    "labelFontSize": "medium",
                    "labelAlign": "center",
                    "valueFontSize": "xlarge-lcd",
                    "valueAlign": "center"
                }]
            }
        ], duration_ms=0)  # Persistent - never expires
        time.sleep(1)

# Example 3: Temporary notification (fire-and-forget)
def show_notification(message, sender):
    display.update_display([
        {
            "type": "textrows",
            "rows": [
                {
                    "label": "",
                    "value": message,
                    "color": "yellow",
                    "valueFontSize": "large",
                    "valueAlign": "center"
                },
                {
                    "label": "from",
                    "value": sender,
                    "color": "white",
                    "labelFontSize": "small",
                    "labelAlign": "center",
                    "valueFontSize": "medium",
                    "valueAlign": "center"
                }
            ]
        }
    ], duration_ms=3000)
    # Returns to idle after 3 seconds automatically
    # Don't call this in a loop!

# Example 4: System stats (respects user duration preference)
display.update_display([
    {"type": "text", "label": "Status", "value": "Active", "color": "green"},
    {"type": "progress", "label": "CPU", "value": 45, "max": 100},
    {"type": "icon", "icon": "warning", "label": "Alerts", "value": "3", "color": "orange"}
])
# duration_ms omitted - uses user's configured duration for this plugin
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
| Windows | `\\.\pipe\MadoMX` |
| macOS/Linux | `~/.config/MadoMX/realtime.sock` |

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
            r"\\.\pipe\MadoMX",
            win32file.GENERIC_WRITE,
            0, None,
            win32file.OPEN_EXISTING,
            0, None
        )
        win32file.WriteFile(pipe, message.encode("utf-8"))
        win32file.CloseHandle(pipe)
    else:
        # Unix socket
        sock_path = Path.home() / ".config" / "MadoMX" / "realtime.sock"
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

    using var pipe = new NamedPipeClientStream(".", "MadoMX", PipeDirection.Out);
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
        sock_path = Path.home() / ".config" / "MadoMX" / "realtime.sock"
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

## Plugin Settings UI (Companion App)

Plugins can provide custom settings controls in the companion app's plugin management interface. Users configure these settings in the UI, and your daemon reads them at runtime.

### How It Works

1. Define a `settingsSchema` in your `companion/manifest.yaml`
2. Companion app renders controls automatically in the plugin detail page
3. User adjusts settings in the UI and clicks "Save Plugin Settings"
4. Settings are saved to `%APPDATA%\MadoMX\plugin_settings.json`
5. Your daemon reads settings from the saved file

### manifest.yaml with Settings Schema

```yaml
pluginId: myvolume
displayName: Volume Control
version: 1.0.0
description: System volume control with custom hotkeys
author: Your Name
autoStart: true
entryPoint: daemon.py

# Settings UI definition
settingsSchema:
  - key: enable_hotkeys
    type: toggle
    label: Enable Hotkeys
    description: Use global hotkeys to control volume
    default: true

  - key: volume_step
    type: select
    label: Volume Step
    description: Volume change per hotkey press
    default: "5"
    options:
      - value: "2"
        label: 2%
      - value: "5"
        label: 5% (Recommended)
      - value: "10"
        label: 10%

  - key: hotkey_prefix
    type: select
    label: Hotkey Modifier
    description: Modifier keys for hotkeys
    default: ctrl+shift
    options:
      - value: ctrl+shift
        label: Ctrl+Shift
      - value: ctrl+alt
        label: Ctrl+Alt
      - value: alt+shift
        label: Alt+Shift
```

### Available Control Types

#### Toggle

Boolean on/off switch.

```yaml
- key: enable_feature
  type: toggle
  label: Enable Feature
  description: Turn this feature on or off
  default: true
```

#### Select (Dropdown)

Dropdown menu with predefined options.

```yaml
- key: output_device
  type: select
  label: Output Device
  description: Audio output device to control
  default: system
  options:
    - value: system
      label: System Default
    - value: speakers
      label: Speakers
    - value: headphones
      label: Headphones
```

**Note**: All values are strings in JSON. Convert to numbers in your daemon if needed.

### Reading Settings in Your Daemon

Settings are saved to a JSON file per plugin:

| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\MadoMX\plugin_settings.json` |
| macOS | `~/.config/MadoMX/plugin_settings.json` |

**File format:**

```json
{
  "myvolume": {
    "enabled": true,
    "settings": {
      "enable_hotkeys": true,
      "volume_step": "5",
      "hotkey_prefix": "ctrl+shift"
    }
  }
}
```

**Python example:**

```python
import json
import os
import sys
from pathlib import Path

# Get settings file path
if sys.platform == "win32":
    SETTINGS_FILE = Path(os.environ.get("APPDATA", "")) / "MadoMX" / "plugin_settings.json"
else:
    SETTINGS_FILE = Path.home() / ".config" / "MadoMX" / "plugin_settings.json"

def load_settings(plugin_id: str) -> dict:
    """Load settings for this plugin."""
    if not SETTINGS_FILE.exists():
        return {}

    try:
        with open(SETTINGS_FILE, "r") as f:
            all_settings = json.load(f)
        plugin_data = all_settings.get(plugin_id, {})
        return plugin_data.get("settings", {})
    except (json.JSONDecodeError, IOError):
        return {}

# Usage
settings = load_settings("myvolume")
hotkeys_enabled = settings.get("enable_hotkeys", True)
volume_step = int(settings.get("volume_step", "5"))
hotkey_prefix = settings.get("hotkey_prefix", "ctrl+shift")

print(f"Hotkeys: {hotkeys_enabled}, Step: {volume_step}%, Prefix: {hotkey_prefix}")
```

### Settings Best Practices

1. **Always provide defaults**: Settings may not exist on first run
2. **Validate user input**: Check for valid ranges and values
3. **Reload on change**: Watch the settings file for changes to support live updates
4. **Keep it simple**: Too many settings overwhelm users
5. **Use descriptions**: Explain what each setting does

### Example: Volume Plugin with Settings

**manifest.yaml:**

```yaml
pluginId: appvolume
displayName: App Volume
version: 1.0.0
description: Per-app volume control with hotkeys
author: Your Name
autoStart: true
entryPoint: daemon.py

settingsSchema:
  - key: show_percentage
    type: toggle
    label: Show Percentage
    description: Display volume as percentage instead of bar
    default: false

  - key: muted_color
    type: select
    label: Muted Color
    description: Color to show when muted
    default: red
    options:
      - value: red
        label: Red (Recommended)
      - value: orange
        label: Orange
      - value: yellow
        label: Yellow
```

**daemon.py:**

```python
import json
import time
from pathlib import Path

SETTINGS_FILE = Path.home() / ".config" / "MadoMX" / "plugin_settings.json"
PLUGIN_ID = "appvolume"

def load_settings():
    if not SETTINGS_FILE.exists():
        return {"show_percentage": False, "muted_color": "red"}

    with open(SETTINGS_FILE, "r") as f:
        all_settings = json.load(f)
    return all_settings.get(PLUGIN_ID, {}).get("settings", {})

def update_display(volume, is_muted):
    settings = load_settings()

    widgets = []

    if settings.get("show_percentage", False):
        # Show as text percentage
        widgets.append({
            "type": "text",
            "label": "Volume",
            "value": f"{volume}%",
            "color": settings.get("muted_color", "red") if is_muted else "cyan"
        })
    else:
        # Show as progress bar
        widgets.append({
            "type": "progress",
            "label": "Volume (MUTED)" if is_muted else "Volume",
            "value": volume,
            "max": 100,
            "color": settings.get("muted_color", "red") if is_muted else "green"
        })

    # Write to display...
    # (See earlier examples for writing JSON)

# Your plugin logic here
while True:
    volume = 75  # Get from audio API
    is_muted = False
    update_display(volume, is_muted)
    time.sleep(0.1)
```

---

## Installing Plugins via Companion App

The companion app provides an "Add Plugin" button that allows users to install plugins from a folder.

### Plugin Folder Structure

Your plugin folder must contain **either**:

1. **Full Logi Plugin** (with companion support):
   ```
   MyPlugin/
   ├── companion/
   │   ├── manifest.yaml      # Required
   │   ├── daemon.py           # Your background script
   │   └── myplugin.json       # Runtime widget data (written by daemon)
   ├── metadata/
   │   ├── LoupedeckPackage.yaml  # Logi plugin metadata
   │   └── Icon256x256.png        # Plugin icon
   ├── MyPlugin.lplug4         # Compiled plugin for Logi Options+
   └── ... (other Logi plugin files)
   ```

2. **Companion-Only Plugin** (no Logi integration):
   ```
   MyPlugin/
   └── companion/
       ├── manifest.yaml      # Required
       ├── daemon.py           # Your background script
       └── myplugin.json       # Runtime widget data (written by daemon)
   ```

### Installation Flow

1. User clicks **"Add Plugin"** in companion app
2. File picker opens to select plugin folder
3. Companion app reads metadata:
   - Checks for `companion/manifest.yaml`
   - Checks for `metadata/LoupedeckPackage.yaml` (if Logi plugin)
   - Extracts plugin name, version, author, description
4. Confirmation dialog shows plugin info
5. User clicks **"Install"**
6. Plugin folder is copied to:
   - Windows: `%LOCALAPPDATA%\Logi\LogiPluginService\Plugins\`
   - macOS: `~/Library/Application Support/Logi/LogiPluginService/Plugins/`
7. Plugin appears in the Plugins list
8. If `autoStart: true`, daemon starts automatically

### After Installation

- **Logi Plugins**: Restart Logi Options+ to load the plugin's console controls
- **Companion Daemons**: Auto-start if `autoStart: true` in manifest
- **Display Data**: Plugin appears in sidebar and can be configured

### Development Workflow

1. Develop your plugin in a working directory
2. Test the daemon by running it manually
3. Use "Add Plugin" to install to the Logi folder
4. Restart Logi Options+ (if using console controls)
5. Verify the daemon auto-starts and display updates work

### Uninstalling Plugins

Currently, plugins must be manually removed:

| Platform | Path |
|----------|------|
| Windows | `%LOCALAPPDATA%\Logi\LogiPluginService\Plugins\YourPlugin\` |
| macOS | `~/Library/Application Support/Logi/LogiPluginService/Plugins/YourPlugin/` |

Delete the plugin folder and restart the companion app.

---

## Plugin Development Guide

### Step 1: Choose Your Plugin Type

**Standalone Plugin**: Simple script that writes JSON files. No Logi Options+ integration.
- ✅ Easy to develop and test
- ✅ Works immediately (no restart needed)
- ❌ No console controls (dials, buttons)

**Companion Plugin**: Background daemon with optional Logi Options+ integration.
- ✅ Auto-starts with companion app
- ✅ Settings UI in companion app
- ✅ Can integrate with Logi console controls
- ❌ Requires installation via companion app

### Step 2: Create Your Plugin Structure

**For standalone plugins:**

```bash
# Windows
cd %APPDATA%\MadoMX\plugins
echo {} > myplugin.json

# macOS
cd ~/.config/MadoMX/plugins
touch myplugin.json
```

**For companion plugins:**

```bash
mkdir MyPlugin
cd MyPlugin
mkdir companion

# Create manifest.yaml
cat > companion/manifest.yaml << EOF
pluginId: myplugin
displayName: My Plugin
version: 1.0.0
description: Description of what your plugin does
author: Your Name
autoStart: true
entryPoint: daemon.py
EOF

# Create daemon.py (see example below)
touch companion/daemon.py
```

### Step 3: Write Your Daemon

**Example: System CPU Monitor**

```python
#!/usr/bin/env python3
"""CPU monitoring plugin for Mado MX."""

import json
import time
import psutil
from pathlib import Path

# Determine companion folder (same directory as this script)
COMPANION_DIR = Path(__file__).parent
PLUGIN_ID = "cpumon"

def update_display(cpu_percent: float, load_avg: tuple):
    """Write widget data to JSON file."""
    widgets = [
        {
            "type": "icon",
            "icon": "cpu",
            "label": "CPU Usage",
            "value": f"{cpu_percent:.1f}%",
            "color": "red" if cpu_percent > 80 else "yellow" if cpu_percent > 50 else "green"
        },
        {
            "type": "progress",
            "label": "Usage",
            "value": int(cpu_percent),
            "max": 100,
            "color": "red" if cpu_percent > 80 else "green"
        }
    ]

    data = {
        "pluginId": PLUGIN_ID,
        "displayName": "CPU Monitor",
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
    print(f"[{PLUGIN_ID}] Starting CPU monitor daemon...")

    while True:
        try:
            cpu_percent = psutil.cpu_percent(interval=1)
            load_avg = psutil.getloadavg() if hasattr(psutil, 'getloadavg') else (0, 0, 0)
            update_display(cpu_percent, load_avg)
        except Exception as e:
            print(f"[{PLUGIN_ID}] Error: {e}")
            time.sleep(5)

if __name__ == "__main__":
    main()
```

### Step 4: Test Locally

**Before installing:**

1. Run your daemon manually:
   ```bash
   cd MyPlugin/companion
   python daemon.py
   ```

2. Check that JSON file is created:
   ```bash
   # Should see myplugin.json being updated
   ls -la
   cat myplugin.json
   ```

3. **For companion plugins**, temporarily copy JSON to standalone plugins folder:
   ```bash
   # Windows
   copy companion\myplugin.json %APPDATA%\MadoMX\plugins\

   # macOS
   cp companion/myplugin.json ~/.config/MadoMX/plugins/
   ```

4. Open companion app and verify your plugin appears in the Plugins list

5. Check the ESP32 display for your widgets

### Step 5: Add Settings UI (Optional)

Edit `companion/manifest.yaml`:

```yaml
pluginId: myplugin
displayName: My Plugin
version: 1.0.0
description: My awesome plugin
author: Your Name
autoStart: true
entryPoint: daemon.py

settingsSchema:
  - key: refresh_interval
    type: select
    label: Refresh Rate
    description: How often to update the display
    default: "1"
    options:
      - value: "0.5"
        label: Fast (0.5s)
      - value: "1"
        label: Normal (1s)
      - value: "5"
        label: Slow (5s)

  - key: show_percentage
    type: toggle
    label: Show Percentage
    description: Display values as percentages
    default: true
```

Update your daemon to read settings:

```python
import json
import os
import sys
from pathlib import Path

if sys.platform == "win32":
    SETTINGS_FILE = Path(os.environ.get("APPDATA", "")) / "MadoMX" / "plugin_settings.json"
else:
    SETTINGS_FILE = Path.home() / ".config" / "MadoMX" / "plugin_settings.json"

def load_settings():
    if not SETTINGS_FILE.exists():
        return {"refresh_interval": "1", "show_percentage": True}

    with open(SETTINGS_FILE, "r") as f:
        all_settings = json.load(f)
    return all_settings.get("myplugin", {}).get("settings", {})

# In your main loop:
settings = load_settings()
refresh_interval = float(settings.get("refresh_interval", "1"))
time.sleep(refresh_interval)
```

### Step 6: Install Plugin

1. **Open companion app**
2. Navigate to **Plugins** section
3. Click **"Add Plugin"**
4. Browse to your `MyPlugin` folder
5. Click **"Install"**
6. Plugin is copied to Logi plugins directory
7. If `autoStart: true`, daemon starts automatically

### Step 7: Debug Issues

**Plugin not appearing?**
- Check companion app console for errors
- Verify `companion/manifest.yaml` is valid YAML
- Ensure folder was copied correctly to Logi plugins directory

**Daemon not starting?**
- Check if `autoStart: true` in manifest
- Verify Python dependencies are installed
- Run daemon manually to see error messages

**Display not updating?**
- Check if JSON file is being written: `companion/myplugin.json`
- Verify JSON is valid (use `json.tool` or online validator)
- Check bridge is connected (green status in companion app)

**Settings not working?**
- Verify settings file exists: `%APPDATA%\MadoMX\plugin_settings.json`
- Check that settings keys match manifest schema
- Reload settings on each loop iteration

### Step 8: Distribute Your Plugin

**Option 1: Folder Distribution**
- Zip your plugin folder
- Users extract and use "Add Plugin" button
- Simple but requires manual installation

**Option 2: Installer Script**
- Create a script that copies files to Logi plugins directory
- Can include dependency installation
- More polished user experience

**Example install script (Windows PowerShell):**

```powershell
# install.ps1
$LOGI_PLUGINS = "$env:LOCALAPPDATA\Logi\LogiPluginService\Plugins"
$PLUGIN_NAME = "MyPlugin"

Write-Host "Installing $PLUGIN_NAME..."

# Create destination
New-Item -ItemType Directory -Force -Path "$LOGI_PLUGINS\$PLUGIN_NAME" | Out-Null

# Copy files
Copy-Item -Path ".\*" -Destination "$LOGI_PLUGINS\$PLUGIN_NAME\" -Recurse -Force

Write-Host "Installation complete!"
Write-Host "Please restart the Mado MX app."
```

**Example install script (macOS/Linux Bash):**

```bash
#!/bin/bash
# install.sh

LOGI_PLUGINS="$HOME/Library/Application Support/Logi/LogiPluginService/Plugins"
PLUGIN_NAME="MyPlugin"

echo "Installing $PLUGIN_NAME..."

# Create destination
mkdir -p "$LOGI_PLUGINS/$PLUGIN_NAME"

# Copy files
cp -R ./* "$LOGI_PLUGINS/$PLUGIN_NAME/"

echo "Installation complete!"
echo "Please restart the Mado MX app."
```

### Plugin Development Tips

1. **Start simple**: Begin with a single text widget, then add complexity
2. **Use atomic writes**: Write to `.tmp` then rename to prevent partial reads
3. **Handle errors gracefully**: Wrap file operations in try/except
4. **Test edge cases**: Low/high values, missing data, disconnected devices
5. **Add logging**: Use `print()` or logging module for debugging
6. **Keep it responsive**: Don't block the main loop with long operations
7. **Document your settings**: Clear descriptions help users understand options
8. **Provide defaults**: Always have fallback values for settings

### Example: Complete Plugin Template

**companion/manifest.yaml:**

```yaml
pluginId: mytemplate
displayName: My Template Plugin
version: 1.0.0
description: Template plugin with all features
author: Your Name
autoStart: true
entryPoint: daemon.py

settingsSchema:
  - key: enabled
    type: toggle
    label: Enable Plugin
    description: Turn plugin on or off
    default: true

  - key: update_interval
    type: select
    label: Update Interval
    description: How often to refresh data
    default: "1"
    options:
      - value: "0.5"
        label: 0.5 seconds
      - value: "1"
        label: 1 second (Recommended)
      - value: "2"
        label: 2 seconds
```

**companion/daemon.py:**

```python
#!/usr/bin/env python3
"""Template daemon for Mado MX plugins."""

import json
import time
import os
import sys
from pathlib import Path

# Constants
COMPANION_DIR = Path(__file__).parent
PLUGIN_ID = "mytemplate"

if sys.platform == "win32":
    SETTINGS_FILE = Path(os.environ.get("APPDATA", "")) / "MadoMX" / "plugin_settings.json"
else:
    SETTINGS_FILE = Path.home() / ".config" / "MadoMX" / "plugin_settings.json"

def load_settings() -> dict:
    """Load plugin settings from companion app."""
    if not SETTINGS_FILE.exists():
        return {"enabled": True, "update_interval": "1"}

    try:
        with open(SETTINGS_FILE, "r") as f:
            all_settings = json.load(f)
        return all_settings.get(PLUGIN_ID, {}).get("settings", {})
    except (json.JSONDecodeError, IOError):
        return {"enabled": True, "update_interval": "1"}

def update_display(widgets: list):
    """Write widget data to JSON file."""
    data = {
        "pluginId": PLUGIN_ID,
        "displayName": "My Template",
        "updated": int(time.time()),
        "widgets": widgets
    }

    plugin_file = COMPANION_DIR / f"{PLUGIN_ID}.json"
    temp_file = plugin_file.with_suffix(".tmp")

    try:
        with open(temp_file, "w") as f:
            json.dump(data, f)
        temp_file.replace(plugin_file)
    except IOError as e:
        print(f"[{PLUGIN_ID}] Error writing display data: {e}")

def main():
    """Main daemon loop."""
    print(f"[{PLUGIN_ID}] Starting daemon...")

    while True:
        try:
            # Load settings
            settings = load_settings()

            # Check if plugin is enabled
            if not settings.get("enabled", True):
                time.sleep(1)
                continue

            # Your plugin logic here
            # Example: Simple counter
            counter = int(time.time()) % 100

            widgets = [
                {
                    "type": "text",
                    "label": "Status",
                    "value": "Running",
                    "color": "green"
                },
                {
                    "type": "progress",
                    "label": "Counter",
                    "value": counter,
                    "max": 100,
                    "color": "cyan"
                }
            ]

            # Update display
            update_display(widgets)

            # Sleep based on settings
            interval = float(settings.get("update_interval", "1"))
            time.sleep(interval)

        except KeyboardInterrupt:
            print(f"\n[{PLUGIN_ID}] Stopping daemon...")
            break
        except Exception as e:
            print(f"[{PLUGIN_ID}] Error: {e}")
            time.sleep(5)  # Wait before retrying

if __name__ == "__main__":
    main()
```

---

## Best Practices

### Duration and Update Strategy

**CRITICAL:** The ESP32 timer starts when your plugin **first displays**, not on every update. Frequent updates will keep the plugin visible but won't reset the timer.

**Three plugin patterns:**

1. **Trigger-Based Plugins (Recommended for most use cases)**
   ```python
   # Write JSON only when user triggers an action
   def on_user_action(volume):
       update_display([
           {"type": "progress", "label": "Volume", "value": volume, "max": 100}
       ])

   # Don't write continuously - let the timer expire naturally
   ```
   - ✅ Timer starts on trigger, expires after duration
   - ✅ Clean transition to idle animation
   - ✅ Use for: Volume controls, notifications, alerts

2. **Persistent Plugins (Always Visible)**
   ```python
   # Set duration_ms: 0 in settings or JSON
   while True:
       data = get_system_stats()
       update_display([
           {"type": "text", "label": "CPU", "value": f"{data.cpu}%"}
       ], duration_ms=0)
       time.sleep(1)
   ```
   - ✅ Stays visible forever (no timer)
   - ✅ Can update as frequently as needed
   - ✅ Use for: System monitors, clocks, persistent dashboards

3. **Self-Clearing Plugins (Send then Clear)**
   ```python
   # Show notification
   update_display([
       {"type": "text", "label": "Alert", "value": "Message received", "color": "yellow"}
   ], duration_ms=3000)

   # ESP32 automatically returns to idle after 3 seconds
   # Don't send updates during this time
   ```
   - ✅ Fire-and-forget notifications
   - ✅ Automatic cleanup
   - ✅ Use for: Toast notifications, temporary alerts

**What NOT to do:**
```python
# ❌ BAD: Continuous updates with non-zero duration
while True:
    volume = get_current_volume()
    update_display([...], duration_ms=2000)  # Timer keeps counting!
    time.sleep(0.1)  # Updates every 100ms
# Problem: Timer counts from first display, so after 2s it goes to idle
# even though you're still sending updates. Confusing behavior!
```

**Best practice for real-time monitoring:**
- If you need continuous updates → Use `duration_ms: 0` (persistent)
- If you want timed display → Only send on user trigger, don't loop

### Update Frequency
- Don't update faster than 10 times per second
- Batch multiple changes into single updates
- Use `updated` timestamp to prevent redundant writes
- **For timed plugins:** Minimize updates after initial trigger to avoid confusion

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

### Duration Timer Not Working

**Plugin stays on screen forever instead of timing out:**

1. **Check plugin sends duration_ms:**
   - Open your plugin's JSON file after it triggers
   - Verify it contains `"duration_ms": 2000` (or your configured value)
   - If missing, the bridge should add it from settings

2. **Check plugin settings:**
   - Open Companion App → Plugins → Your Plugin
   - Look for "Display Duration (ms)" setting
   - Set to your desired duration (e.g., 2000 for 2 seconds)
   - Click "Save Plugin Settings"

3. **Check global default:**
   - Settings → Display → Default Plugin Duration
   - Set to your desired default (e.g., 2000)
   - This applies to plugins without their own duration setting

4. **Clear cached data:**
   - Close companion app
   - Delete `%APPDATA%\MadoMX\plugins\*.json` (Windows)
   - Delete `~/.config/MadoMX/plugins/*.json` (macOS)
   - Restart app and trigger plugin fresh

5. **Check plugin update pattern:**
   - If your plugin sends updates continuously (every 100ms), the timer won't help
   - Solution: Only write JSON file when user triggers action, not in a loop
   - See "Duration and Update Strategy" in Best Practices above

**Plugin disappears too quickly:**

1. **Check if plugin is sending multiple triggers:**
   - Each new plugin display resets the timer
   - Solution: Avoid writing JSON file repeatedly for same action

2. **Increase duration:**
   - Plugin Settings → Display Duration → Increase value
   - Or set to `0` for persistent (never expires)

### Touch Not Working

1. Verify widgets are in touchable zones (grid in bottom, list in main)
2. Check the touch events file is being written
3. Touch requires firm press on CYD screen

### Connection Issues

1. Ensure only one application uses the serial port
2. Close the bridge before flashing new ESP32 firmware
3. Try reconnecting via the config app
