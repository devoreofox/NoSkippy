using System;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Plugin.Services;

namespace NoSkippy.Services;

public sealed class DtrIndicator : IDisposable
{
    private readonly IFramework framework;
    private readonly Configuration configuration;
    private readonly PluginState state;
    private readonly IDtrBarEntry entry;

    public DtrIndicator(IFramework framework, IDtrBar dtrBar, Configuration configuration, PluginState state)
    {
        this.framework = framework;
        this.configuration = configuration;
        this.state = state;

        entry = dtrBar.Get("No Skippy");
        entry.Shown = false;
        framework.Update += OnUpdate;
    }

    public void Dispose()
    {
        framework.Update -= OnUpdate;
        entry.Remove();
    }

    private void OnUpdate(IFramework _)
    {
        entry.Shown = configuration.ShowDtr;
        if (!entry.Shown) return;

        if (state.Active)
        {
            entry.Text = "No Skippy: On";
            entry.Tooltip = "No Skippy is active.";
        }
        else if (state.MasterOff)
        {
            entry.Text = "No Skippy: Off";
            entry.Tooltip = "No Skippy is switched off.";
        }
        else if (state.PausedForCutscene)
        {
            entry.Text = "No Skippy: Paused";
            entry.Tooltip = "Paused until this cutscene ends.";
        }
        else
        {
            var left = state.PauseRemaining;
            entry.Text = $"No Skippy: Paused ({left.Minutes}:{left.Seconds:00})";
            entry.Tooltip = "Paused for a short while.";
        }
    }
}
