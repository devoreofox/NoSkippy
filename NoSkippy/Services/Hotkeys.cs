using System;
using Dalamud.Plugin.Services;

namespace NoSkippy.Services;

public sealed class Hotkeys : IDisposable
{
    private readonly IFramework framework;
    private readonly IKeyState keyState;
    private readonly Configuration configuration;
    private readonly PluginState state;
    private readonly Func<bool> suppressed;

    private bool pauseWasDown;
    private bool masterWasDown;

    public Hotkeys(IFramework framework, IKeyState keyState, Configuration configuration, PluginState state, Func<bool> suppressed)
    {
        this.framework = framework;
        this.keyState = keyState;
        this.configuration = configuration;
        this.state = state;
        this.suppressed = suppressed;

        framework.Update += OnUpdate;
    }

    public void Dispose() => framework.Update -= OnUpdate;

    private void OnUpdate(IFramework _)
    {
        var pauseDown = !suppressed() && KeyCombo.Held(keyState, configuration.PauseKeys);
        var masterDown = !suppressed() && KeyCombo.Held(keyState, configuration.MasterOffKeys);

        if (pauseDown && !pauseWasDown) state.TogglePause();
        if (masterDown && !masterWasDown) state.ToggleMaster();

        pauseWasDown = pauseDown;
        masterWasDown = masterDown;
    }
}
