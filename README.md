# Tarkov Keybinds for Logitech MX Creative Console

This plugin maps Tarkov's keybinds to the MX Creative Console so you can trigger them with a single button press.

It exists because Tarkov parses the keypresses in a way that the usual keyboard binds on most decks wont work.

## What's included

The plugin covers most of Tarkov's controls:

- **Movement** — Leaning, smooth leaning, sidesteps, crouch, prone, blind fire positions
- **Weapons** — Reload, fire mode, check ammo/chamber, detach mag, inspect, fold stock
- **Tactical gear** — Flashlight/laser toggles, helmet lights, NVGs, face shields
- **Quick slots** — Weapon slots 1-3 plus quick slots 4-9
- **Utility** — Inventory, compass, voice commands, discard, drop backpack

## Setup

1. Install the plugin through Logi Options+ or drop it in your Loupedeck plugins folder
2. Add the actions you want to your console layout
3. Make sure Tarkov is using default keybinds (or adjust accordingly)

The plugin sends the actual key combinations to Windows, so Tarkov sees them as regular keyboard input. No game files are modified.

## Mado MX Display Integration

If you have a [Mado MX](https://github.com/user/mado-mx) setup (ESP32-CYD + companion app), the plugin will show keybind info on your screen whenever a button is pressed.

**What it shows:**

- **Label** — The action name (e.g. "Check Ammo")
- **Value** — The key combination (e.g. "LAlt+T")

The display appears for the duration set in your Mado MX settings (default 2 seconds), then returns to your idle animation.

### How it works

When you press a button on the console, the plugin writes a JSON file to:

```
%LOCALAPPDATA%\Logi\LogiPluginService\Plugins\TarkovKeybindPlugin\companion\tarkovkeybinds.json
```

The Mado MX bridge watches this folder and sends the widget data to the ESP32 over serial. The JSON format uses the `textrows` widget type:

```json
{
  "pluginId": "tarkovkeybinds",
  "displayName": "Tarkov Keybinds",
  "updated": 1707000000,
  "widgets": [
    {
      "type": "textrows",
      "rows": [
        {
          "label": "Check Ammo",
          "value": "LAlt+T",
          "color": "orange",
          "labelFontSize": "small",
          "valueFontSize": "xlarge",
          "labelAlign": "center",
          "valueAlign": "center"
        }
      ]
    }
  ]
}
```

### Display settings

Open the Mado MX companion app, go to the **Tarkov Keybinds** plugin page, and you'll find these options under Plugin Settings:

| Setting | Options | Default |
|---------|---------|---------|
| Display Color | Orange, White, Cyan, Green, Yellow, Red, Magenta, Blue | Orange |
| Key Font Size | Extra Large, Large, Medium, Small | Extra Large |
| Label Font Size | Small, Medium, Large | Small |
| Key Alignment | Center, Left, Right | Center |
| Label Alignment | Center, Left, Right | Center |

Changes apply immediately — the next button press will use the new settings. These are declared via `settingsSchema` in `companion/manifest.yaml` and stored by Mado MX in `%APPDATA%\MadoMX\plugin_settings.json`.

## Note on screenshots

Tarkov's screenshot key (PrtScn) doesn't work reliably when sent programmatically. If you want the screenshot button to work, rebind it in Tarkov's settings to just "Print" instead of "PrtScn".

## Building

```bash
cd TarkovKeybindPlugin
dotnet build -c Release
```

This builds the DLL, copies it to the Logi plugins folder, and triggers a hot reload.

**Output:** `bin\EFTKeybinds\`
**Deploy target:** `%LOCALAPPDATA%\Logi\LogiPluginService\Plugins\TarkovKeybindPlugin\`

## Project structure

```
TarkovKeybindPlugin/
  src/
    TarkovKeybindPlugin.cs     # Main plugin entry point
    TarkovKeybindApplication.cs # Application stub (required by Logi)
    KeybindCommands.cs          # All keybind command classes
    KeySender.cs                # Windows SendInput wrapper
    CompanionDisplayClient.cs   # Mado MX JSON file writer
    DisplayConfig.cs            # Display settings (font, color, alignment)
    PluginLog.cs                # Simple file logger
  companion/
    manifest.yaml               # Companion metadata for Mado MX
    daemon.py                   # Companion daemon (placeholder)
  package/
    metadata/
      Icon256x256.png           # Plugin icon
      LoupedeckPackage.yaml     # Package metadata
  pluginconfig.yaml             # Logi plugin configuration
```

## License

MIT
