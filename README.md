# No Skippy

A Dalamud plugin for FFXIV that slows down dialogue and makes cutscene skipping take a little effort, so the story gets read instead of clicked through.

## Features

- **Dialogue lock** - Spam-clicking or hammering the advance key through dialogue boxes engages a lock that swallows your input until you stop for a moment. A red lock replaces the advance arrow while it is active, and hovering it tells you why: the text is still being written, there is a delay to read, or you are clicking too fast.
- **Input delay** - A new dialogue box ignores input for a short time after it appears or its text changes. Clicking during the delay restarts it, so you have to actually stop clicking.
- **Wait for text** - Optionally ignore input until the text has finished writing out, then for the input delay after that.
- **Skip guard** - Choosing Yes on the "Skip cutscene?" prompt only works while the skip combo is held (Ctrl+Shift by default). A progress ring follows your cursor while you hold it, with a configurable hold time. Optionally block skipping entirely.
- **Bypass** - Holding the skip combo lets dialogue through immediately, ignoring the lock, the wait and the delay.
- **Pause and master off** - Pause No Skippy for the rest of a cutscene, or for a set number of minutes elsewhere, or switch it off entirely until your next login.
- **Info bar entry** - Shows whether No Skippy is on, off or paused, with the time remaining on a timed pause.
- **Per-surface toggles** - Choose whether the dialogue lock applies in cutscenes, outside cutscenes, or both.
- **Rebindable keys** - Every keybind is rebound by clicking it in the settings and pressing the new keys.

## Installation

1. Open XIVLauncher settings
2. Navigate to **Dalamud** → **Custom Plugin Repositories**
3. Add the following URL:
```
https://raw.githubusercontent.com/devoreofox/NoSkippy/main/repo.json
```
4. Open `/xlplugins` in-game and search for **No Skippy**

## Usage

Type `/noskippy` in-game to open the settings. Pick a category from the left panel: Dialogue, Cutscene skipping, Keybinds or Feedback.

### Keybinds

All of these can be rebound in the Keybinds page.

- `Ctrl+Shift` - Skip combo. Hold it to confirm the skip prompt, and to bypass the dialogue lock
- `Ctrl+Alt+P` - Pause. In a cutscene it pauses until the cutscene ends, elsewhere for the pause length (2 minutes by default). Press again to cancel
- `Ctrl+Alt+O` - Master off. Switches No Skippy off or on without unloading it, and resets to on at login

### Commands

- `/noskippy` - Open the settings

## Contributing

This plugin is a personal project. If you have an idea, feel free to open an issue!

## License

No Skippy is licensed under [AGPL-3.0](LICENSE.md).

## Attribution

No Skippy is free software under the AGPL-3.0, and attribution is required. If you fork it, redistribute it, or reuse its code, you must keep the existing copyright and license notices in place and credit the original author (Oreo) with a visible link back to this repository (https://github.com/devoreofox/NoSkippy). Please do not remove or hide that attribution.
