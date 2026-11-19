using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;

namespace NoSkippy.Services;

public sealed class PluginState : IDisposable
{
    private readonly ICondition condition;
    private readonly IClientState clientState;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    private bool pausedForCutscene;
    private DateTime pausedUntil;

    public PluginState(ICondition condition, IClientState clientState, IChatGui chatGui, Configuration configuration)
    {
        this.condition = condition;
        this.clientState = clientState;
        this.chatGui = chatGui;
        this.configuration = configuration;

        condition.ConditionChange += OnConditionChange;
        clientState.Login += OnLogin;
    }

    public bool MasterOff { get; private set; }
    public bool PausedForCutscene => pausedForCutscene;
    public bool Paused => pausedForCutscene || DateTime.UtcNow < pausedUntil;
    public bool Active => !MasterOff && !Paused;
    public TimeSpan PauseRemaining => pausedForCutscene ? TimeSpan.Zero : pausedUntil - DateTime.UtcNow;

    public void Dispose()
    {
        condition.ConditionChange -= OnConditionChange;
        clientState.Login -= OnLogin;
    }

    public void ToggleMaster()
    {
        MasterOff = !MasterOff;
        Say(MasterOff ? "Switched off." : "Switched on.");
    }

    public void TogglePause()
    {
        if (Paused)
        {
            pausedForCutscene = false;
            pausedUntil = default;
            Say("Pause cancelled.");
        }
        else if (condition[ConditionFlag.OccupiedInCutSceneEvent])
        {
            pausedForCutscene = true;
            Say("Paused until the cutscene ends.");
        }
        else
        {
            pausedUntil = DateTime.UtcNow.AddMinutes(configuration.PauseMinutes);
            Say($"Paused for {configuration.PauseMinutes} minutes.");
        }
    }

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        if (flag != ConditionFlag.OccupiedInCutSceneEvent || value || !pausedForCutscene) return;

        pausedForCutscene = false;
        Say("Cutscene over, active again.");
    }

    private void OnLogin() => MasterOff = false;

    private void Say(string message)
    {
        if (configuration.ChatMessages) chatGui.Print($"[NoSkippy] {message}");
    }
}
