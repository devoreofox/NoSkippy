# No Skippy

A Dalamud plugin for FFXIV that slows down dialogue and makes skipping a cutscene take some effort, so you read the story instead of clicking through it.

## Features

- **Dialogue lock** - If you spam click through dialogue, No Skippy stops your input until you pause for a moment. A red lock covers the advance arrow while it is active. Hover it to see why: the text is still being written, there is a delay to read, or you are clicking too fast.
- **Input delay** - A new dialogue box ignores input for a short time after it appears or its text changes. Clicking during the delay restarts it, so you have to actually stop clicking.
- **Wait for text** - Optionally ignore input until the text has finished writing out, and then for the input delay after that.
- **Skip guard** - Yes on the "Skip cutscene?" prompt only works while you hold the skip combo (Ctrl+Shift by default). A ring follows your cursor to show progress, and the hold time is configurable. You can also block skipping entirely.
- **Bypass** - Hold the skip combo to advance dialogue immediately, ignoring the lock, the wait and the delay.
- **Pause and master off** - Pause No Skippy until the end of a cutscene, or for a set number of minutes elsewhere. Master off turns it off completely until your next login.
- **Info bar entry** - Shows whether No Skippy is on, off or paused, and how long a timed pause has left.
- **Per-surface toggles** - Apply the dialogue lock in cutscenes, outside them, or both.
- **Rebindable keys** - Click a keybind in the settings and press the new keys.

## Installation

1. Open XIVLauncher settings
2. Navigate to **Dalamud** → **Custom Plugin Repositories**
3. Add the following URL:
```
https://raw.githubusercontent.com/devoreofox/NoSkippy/main/repo.json
```
4. Open `/xlplugins` in-game and search for **No Skippy**

## Usage

Type `/noskippy` in-game to open the settings. The categories on the left are Dialogue, Cutscene skipping, Keybinds and Feedback.

### Keybinds

You can rebind all of these on the Keybinds page.

- `Ctrl+Shift` - Skip combo. Hold it to confirm the skip prompt or to bypass the dialogue lock
- `Ctrl+Alt+P` - Pause. In a cutscene it pauses until the cutscene ends. Elsewhere it pauses for the pause length (2 minutes by default). Press it again to cancel
- `Ctrl+Alt+O` - Master off. Switches No Skippy off or on without unloading it. It resets to on at login

### Commands

- `/noskippy` - Open the settings

## Contributing

This plugin is a personal project. If you have an idea, feel free to open an issue!

## License

No Skippy is licensed under [AGPL-3.0](LICENSE.md).

## Attribution

No Skippy is free software under the AGPL-3.0, and attribution is required. If you fork it, redistribute it, or reuse its code, you must keep the existing copyright and license notices in place and credit the original author (Oreo) with a visible link back to this repository (https://github.com/devoreofox/NoSkippy). Please do not remove or hide that attribution.
