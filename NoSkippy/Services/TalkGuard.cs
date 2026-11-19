using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Addon.Events;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace NoSkippy.Services;

public sealed unsafe class TalkGuard : IDisposable
{
    private readonly IAddonLifecycle lifecycle;
    private readonly Configuration configuration;
    private readonly PluginState state;
    private readonly SpamLock spamLock;
    private bool swallowingClick;
    private long shownAt;
    private long lastDownAt;
    private int lastTextHash;
    private long lastSwallowedAt = -100000;
    private long textDoneAt;
    private readonly IKeyState keyState;
    private readonly ICondition condition;
    private readonly IGameGui gameGui;

    public TalkGuard(IAddonLifecycle lifecycle, Configuration configuration, PluginState state, ICondition condition, IGameGui gameGui, IKeyState keyState)
    {
        this.lifecycle = lifecycle;
        this.configuration = configuration;
        this.state = state;
        this.condition = condition;
        this.gameGui = gameGui;
        this.keyState = keyState;
        spamLock = new SpamLock(configuration);

        lifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "Talk", OnReceiveEvent);
        lifecycle.RegisterListener(AddonEvent.PreFinalize, "Talk", OnFinalize);
        lifecycle.RegisterListener(AddonEvent.PostSetup, "Talk", OnShown);
        lifecycle.RegisterListener(AddonEvent.PostRefresh, "Talk", OnRefresh);
    }

    public bool Locked => spamLock.Locked;

    public void Dispose()
    {
        lifecycle.UnregisterListener(AddonEvent.PreReceiveEvent, "Talk", OnReceiveEvent);
        lifecycle.UnregisterListener(AddonEvent.PreFinalize, "Talk", OnFinalize);
        lifecycle.UnregisterListener(AddonEvent.PostSetup, "Talk", OnShown);
        lifecycle.UnregisterListener(AddonEvent.PostRefresh, "Talk", OnRefresh);
    }

    private void OnFinalize(AddonEvent type, AddonArgs args)
    {
        spamLock.Reset();
    }

    public void Draw()
    {
        if (!state.Active || !configuration.ShowTalkLock || !configuration.TalkLockEnabled || !SurfaceEnabled()) return;

        var now = Environment.TickCount64;
        if (Bypass()) return;
        var reason = BlockReason(now);
        if (reason == null) return;

        var addon = (AtkUnitBase*)gameGui.GetAddonByName("Talk").Address;
        if (addon == null || !addon->IsVisible) return;

        var arrow = addon->UldManager.SearchNodeById(7);
        if (arrow == null) return;
        var center = new Vector2(arrow->ScreenX + arrow->Width * addon->Scale * 0.5f, arrow->ScreenY + arrow->Height * addon->Scale * 0.5f);
        var scale = configuration.RingScale;
        var draw = ImGui.GetForegroundDrawList();

        draw.AddCircleFilled(center, 15 * scale, 0xFFD0E0E8, 24);
        ImGuiUtil.DrawLock(draw, center + new Vector2(0, -3 * scale), scale, 0xFF2828E0);

        if (Vector2.DistanceSquared(ImGui.GetIO().MousePos, center) <= 15 * scale * 15 * scale)
            ImGui.SetTooltip(reason);
    }

    private void OnShown(AddonEvent type, AddonArgs args)
    {
        shownAt = Environment.TickCount64;
        textDoneAt = 0;
        lastTextHash = 0;
    }

    private void OnRefresh(AddonEvent type, AddonArgs args)
    {
        if (args is not AddonRefreshArgs refresh) return;

        var values = (AtkValue*)refresh.AtkValues;
        var hash = 17;
        for (var i = 0; i < refresh.AtkValueCount; i++)
            if (values[i].Type is AtkValueType.String or AtkValueType.ManagedString)
                hash = hash * 31 + values[i].String.ToString().GetHashCode();

        if (hash == lastTextHash) return;
        lastTextHash = hash;
        shownAt = Environment.TickCount64;
        textDoneAt = 0;
    }

    private void OnReceiveEvent(AddonEvent type, AddonArgs args)
    {
        if (!state.Active || !configuration.TalkLockEnabled || !SurfaceEnabled() || Bypass() || args is not AddonReceiveEventArgs e) return;

        switch (e.AtkEventType)
        {
            case AddonEventType.MouseDown:
            case AddonEventType.InputReceived:
                var now = Environment.TickCount64;
                var repeat = now - lastDownAt < 80;
                lastDownAt = now;
                if (repeat) break;

                var tooSoon = DelayReason(now) != null;
                if (tooSoon)
                {
                    shownAt = now;
                    if (textDoneAt != 0) textDoneAt = now;
                }
                swallowingClick = spamLock.ShouldSwallow(now) | tooSoon;
                break;
            case AddonEventType.MouseUp:
            case AddonEventType.MouseClick:
                break;
            default:
                return;
        }

        if (!swallowingClick) return;
        lastSwallowedAt = Environment.TickCount64;
        e.AtkEventType = (AddonEventType)0;
    }

    private bool Bypass() => configuration.SkipComboBypassesTalk && KeyCombo.Held(keyState, configuration.SkipComboKeys);

    private string? BlockReason(long now)
    {
        return DelayReason(now) ?? (now - lastSwallowedAt <= configuration.LockReleaseMs
            ? "Clicking too fast. Stop clicking for a moment."
            : null);
    }

    private string? DelayReason(long now)
    {
        if (configuration.WaitForText && now - shownAt <= 10000)
        {
            var addon = (AtkUnitBase*)gameGui.GetAddonByName("Talk").Address;
            var arrow = addon == null ? null : addon->UldManager.SearchNodeById(7);
            if (arrow == null || !arrow->IsVisible())
            {
                textDoneAt = 0;
                return "The text is still being written.";
            }

            if (textDoneAt == 0) textDoneAt = now;
            if (now - textDoneAt < configuration.TalkDelayMs) return "Delay to read. Clicking restarts the delay.";
        }

        return now - shownAt < configuration.TalkDelayMs ? "New text to read. Clicking restarts the delay." : null;
    }

    private bool SurfaceEnabled() => condition[ConditionFlag.OccupiedInCutSceneEvent]
        ? configuration.TalkLockInCutscenes
        : configuration.TalkLockOutsideCutscenes;
}
