using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace NoSkippy.Services;

public sealed unsafe class SkipGuard : IDisposable
{
    private delegate bool FireCallbackDelegate(AtkUnitBase* addon, uint valueCount, AtkValue* values, bool close);

    private readonly IAddonLifecycle lifecycle;
    private readonly IKeyState keyState;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;
    private readonly PluginState state;
    private readonly Hook<FireCallbackDelegate> hook;
    private readonly Stopwatch clock = Stopwatch.StartNew();

    private AtkUnitBase* addon;
    private bool promptOpen;
    private long heldSince = -1;

    public SkipGuard(IAddonLifecycle lifecycle, IKeyState keyState, IChatGui chatGui, IGameInteropProvider interop, Configuration configuration, PluginState state)
    {
        this.state = state;
        this.lifecycle = lifecycle;
        this.keyState = keyState;
        this.chatGui = chatGui;
        this.configuration = configuration;

        lifecycle.RegisterListener(AddonEvent.PostSetup, "SelectString", OnSetup);
        lifecycle.RegisterListener(AddonEvent.PreFinalize, "SelectString", OnFinalize);

        hook = interop.HookFromAddress<FireCallbackDelegate>((nint)AtkUnitBase.MemberFunctionPointers.FireCallback, OnFireCallback);
        hook.Enable();
    }

    public void Dispose()
    {
        hook.Dispose();
        lifecycle.UnregisterListener(AddonEvent.PostSetup, "SelectString", OnSetup);
        lifecycle.UnregisterListener(AddonEvent.PreFinalize, "SelectString", OnFinalize);
    }

    public void Draw()
    {
        if (!promptOpen || !configuration.SkipGuardEnabled || !state.Active) return;

        if (ComboHeld())
        {
            if (heldSince < 0) heldSince = clock.ElapsedMilliseconds;
        }
        else
        {
            heldSince = -1;
        }

        var center = ImGui.GetIO().MousePos + new Vector2(configuration.RingOffsetX, configuration.RingOffsetY);
        var draw = ImGui.GetForegroundDrawList();

        if (HoveringYes() && !SkipAllowed()) ImGuiUtil.DrawLock(draw, center, configuration.RingScale);

        if (configuration.SkipHoldMs <= 0 || heldSince < 0) return;

        var progress = Math.Clamp((clock.ElapsedMilliseconds - heldSince) / (float)configuration.SkipHoldMs, 0f, 1f);
        var radius = 14 * configuration.RingScale;
        draw.AddCircle(center, radius, 0x80FFFFFF, 32, 3 * configuration.RingScale);
        if (progress <= 0f) return;

        draw.PathArcTo(center, radius, -MathF.PI / 2, -MathF.PI / 2 + MathF.Tau * progress, 32);
        draw.PathStroke(progress >= 1f ? 0xFF50E050 : 0xFF50C8FF, ImDrawFlags.None, 4 * configuration.RingScale);
    }

    private bool HoveringYes()
    {
        if (addon == null) return false;
        var list = ((AddonSelectString*)addon)->PopupMenu.List;
        return list != null && list->HoveredItemIndex == 0;
    }

    private void OnSetup(AddonEvent type, AddonArgs args)
    {
        if (args is not AddonSetupArgs setup || setup.AtkValueCount < 3) return;

        var title = ((AtkValue*)setup.AtkValues)[2].String.ToString();
        promptOpen = title == configuration.SkipPromptText;
        addon = (AtkUnitBase*)args.Addon.Address;
        heldSince = -1;
    }

    private void OnFinalize(AddonEvent type, AddonArgs args)
    {
        promptOpen = false;
        addon = null;
        heldSince = -1;
    }

    private bool ComboHeld()
    {
        return KeyCombo.Held(keyState, configuration.SkipComboKeys);
    }

    private bool OnFireCallback(AtkUnitBase* addon, uint valueCount, AtkValue* values, bool close)
    {
        var isYes = promptOpen && state.Active && configuration.SkipGuardEnabled && valueCount > 0 && values[0].Int == 0 && addon->NameString == "SelectString";
        if (!isYes || SkipAllowed()) return hook.Original(addon, valueCount, values, close);

        chatGui.Print("[NoSkippy] Hold the skip combo to skip.");
        return false;
    }

    private bool SkipAllowed()
    {
        if (configuration.NeverAllowSkip || !ComboHeld()) return false;
        return configuration.SkipHoldMs <= 0 || (heldSince >= 0 && clock.ElapsedMilliseconds - heldSince >= configuration.SkipHoldMs);
    }
}
