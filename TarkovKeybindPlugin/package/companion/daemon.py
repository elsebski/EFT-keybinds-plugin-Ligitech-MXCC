#!/usr/bin/env python3
"""Tarkov Keybinds companion daemon.

This daemon integrates with the MX Companion Display to show keybind status
and receive touch input. Currently a placeholder - set autoStart: true in
manifest.yaml when ready to use.
"""

import json
import time
from pathlib import Path

# Companion folder (same directory as this script)
COMPANION_DIR = Path(__file__).parent
PLUGIN_ID = "tarkovkeybind"
DISPLAY_NAME = "Tarkov Keybinds"


def update_display(widgets: list):
    """Write widget data to JSON file for the companion display."""
    data = {
        "pluginId": PLUGIN_ID,
        "displayName": DISPLAY_NAME,
        "updated": int(time.time()),
        "widgets": widgets
    }

    plugin_file = COMPANION_DIR / f"{PLUGIN_ID}.json"
    temp_file = plugin_file.with_suffix(".tmp")

    # Atomic write to prevent partial reads
    with open(temp_file, "w") as f:
        json.dump(data, f)
    temp_file.replace(plugin_file)


def main():
    """Main daemon loop."""
    # Initial display - show plugin is ready
    update_display([
        {"type": "text", "label": "Status", "value": "Ready", "color": "green"},
        {"type": "text", "label": "Game", "value": "Tarkov", "color": "cyan"}
    ])

    # Main loop - extend this with actual functionality
    while True:
        time.sleep(1)


if __name__ == "__main__":
    main()
