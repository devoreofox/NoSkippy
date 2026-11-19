using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using NoSkippy.Services;

namespace NoSkippy.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly IKeyState keyState;

    private string? capturingLabel;
    private Func<List<VirtualKey>>? getTarget;
    private Action<List<VirtualKey>>? setTarget;
    private readonly HashSet<VirtualKey> captured = new();

    private static readonly Vector4 Accent = new(0.7f, 0.5f, 1.0f, 1.0f);
    private static readonly string[] Categories = { "Dialogue", "Cutscene skipping", "Keybinds", "Feedback" };
    private int selected;

    public ConfigWindow(Plugin plugin, IKeyState keyState)
        : base("No Skippy###NoSkippy", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 380),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        configuration = plugin.Configuration;
        this.keyState = keyState;
    }

    public bool Capturing => capturingLabel != null;

    public void Dispose() { }

    public override void OnClose() => capturingLabel = null;

    public override void Draw()
    {
        var scale = ImGui.GetIO().FontGlobalScale;

        ImGui.BeginChild("###categories", new Vector2(180 * scale, 0), true);
        for (var i = 0; i < Categories.Length; i++)
            if (ImGui.Selectable(Categories[i], selected == i)) selected = i;
        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("###panel", new Vector2(0, 0), true);
        ImGui.TextColored(Accent, Categories[selected]);
        ImGui.Separator();
        ImGui.Spacing();

        switch (selected)
        {
            case 0: DrawDialogue(); break;
            case 1: DrawSkipping(); break;
            case 2: DrawKeybinds(); break;
            default: DrawFeedback(); break;
        }

        ImGui.EndChild();

        if (capturingLabel != null) PollCapture();
    }

    private void DrawDialogue()
    {
        ImGui.TextWrapped("Stops you spam-clicking through dialogue boxes without reading them.");
        ImGui.Spacing();
        Toggle("Lock rapid clicking", configuration.TalkLockEnabled, v => configuration.TalkLockEnabled = v,
            "Swallows clicks and key presses while you are advancing dialogue too fast, until you stop for a moment.");
        Toggle("Apply in cutscenes", configuration.TalkLockInCutscenes, v => configuration.TalkLockInCutscenes = v,
            "Dialogue boxes shown during cutscenes are guarded.");
        Toggle("Apply outside cutscenes", configuration.TalkLockOutsideCutscenes, v => configuration.TalkLockOutsideCutscenes = v,
            "Ordinary NPC dialogue boxes are guarded.");
        Toggle("Skip combo bypasses lock", configuration.SkipComboBypassesTalk, v => configuration.SkipComboBypassesTalk = v,
            "Holding the skip combo lets you advance dialogue immediately, ignoring the lock, the text wait and the delay.");
        Toggle("Show lock on dialogue box", configuration.ShowTalkLock, v => configuration.ShowTalkLock = v,
            "Draws a lock on the dialogue box while your clicks are being swallowed.");
        Slider("Rapid click gap (ms)", configuration.RapidClickMs, 100, 2000, v => configuration.RapidClickMs = v,
            "Two advances closer together than this count as spamming and engage the lock.");
        Slider("Lock release gap (ms)", configuration.LockReleaseMs, 100, 3000, v => configuration.LockReleaseMs = v,
            "While locked, you must stop clicking for this long before input works again.");
        Toggle("Wait for text to finish", configuration.WaitForText, v => configuration.WaitForText = v,
            "Input is ignored until the text has finished typing out, then for the input delay below. Gives up after 10 seconds.");
        Slider("New box input delay (ms)", configuration.TalkDelayMs, 0, 2000, v => configuration.TalkDelayMs = v,
            "Input is ignored for this long after a box appears, its text changes, or (with wait for text) the text finishes. Clicking during the delay restarts it.");
    }

    private void DrawSkipping()
    {
        ImGui.TextWrapped("Makes skipping a cutscene take deliberate effort.");
        ImGui.Spacing();
        Toggle("Require skip combo", configuration.SkipGuardEnabled, v => configuration.SkipGuardEnabled = v,
            "Choosing Yes on the skip prompt only works while the skip combo is held.");
        Toggle("Never allow skipping", configuration.NeverAllowSkip, v => configuration.NeverAllowSkip = v,
            "Blocks Yes on the skip prompt entirely, even with the combo held.");
        Slider("Hold time (ms)", configuration.SkipHoldMs, 0, 5000, v => configuration.SkipHoldMs = v,
            "How long the combo must be held. 0 means no hold and no ring.");
        Slider("Ring offset X", configuration.RingOffsetX, -100, 100, v => configuration.RingOffsetX = v,
            "Horizontal position of the ring and lock relative to the cursor.");
        Slider("Ring offset Y", configuration.RingOffsetY, -100, 100, v => configuration.RingOffsetY = v,
            "Vertical position of the ring and lock relative to the cursor.");

        var size = configuration.RingScale;
        if (ImGui.SliderFloat("Ring and lock size", ref size, 0.5f, 4f))
        {
            configuration.RingScale = size;
            configuration.Save();
        }
    }

    private void DrawKeybinds()
    {
        ImGui.TextWrapped("Click a button, hold the new keys, then release. Esc cancels.");
        ImGui.Spacing();
        Keybind("Skip combo", () => configuration.SkipComboKeys, v => configuration.SkipComboKeys = v,
            "Held to confirm the skip prompt.");
        Keybind("Pause", () => configuration.PauseKeys, v => configuration.PauseKeys = v,
            "In a cutscene, pauses No Skippy until it ends. Elsewhere, pauses for the length below. Press again to cancel.");
        Keybind("Master off", () => configuration.MasterOffKeys, v => configuration.MasterOffKeys = v,
            "Switches No Skippy off or on without unloading it. Resets to on at login.");
        Slider("Pause length (minutes)", configuration.PauseMinutes, 1, 30, v => configuration.PauseMinutes = v,
            "How long a pause lasts when pressed outside a cutscene.");
    }

    private void DrawFeedback()
    {
        ImGui.TextWrapped("Choose how No Skippy tells you what it is doing.");
        ImGui.Spacing();
        Toggle("Chat messages", configuration.ChatMessages, v => configuration.ChatMessages = v,
            "Prints a chat message when you pause, cancel, switch off or on.");
        Toggle("Info bar entry", configuration.ShowDtr, v => configuration.ShowDtr = v,
            "Shows No Skippy's state in the info bar: active, paused or off.");
    }

    private void Toggle(string label, bool value, Action<bool> set, string tip)
    {
        var changed = ImGui.Checkbox(label, ref value);
        ImGuiUtil.Tooltip(tip);
        if (!changed) return;
        set(value);
        configuration.Save();
    }

    private void Slider(string label, int value, int min, int max, Action<int> set, string tip)
    {
        var changed = ImGui.SliderInt(label, ref value, min, max);
        ImGuiUtil.Tooltip(tip);
        if (!changed) return;
        set(value);
        configuration.Save();
    }

    private void Keybind(string label, Func<List<VirtualKey>> get, Action<List<VirtualKey>> set, string tip)
    {
        var active = capturingLabel == label;
        var text = active ? "Press keys, Esc cancels..." : $"{KeyCombo.Format(get())}  (click to rebind)";
        ImGui.TextUnformatted(label);
        ImGui.SameLine(130 * ImGui.GetIO().FontGlobalScale);
        var clicked = ImGui.Button($"{text}##{label}");
        ImGuiUtil.Tooltip(tip);
        if (!clicked) return;

        capturingLabel = label;
        getTarget = get;
        setTarget = set;
        captured.Clear();
    }

    private void PollCapture()
    {
        if (keyState[VirtualKey.ESCAPE])
        {
            capturingLabel = null;
            return;
        }

        var down = keyState.GetValidVirtualKeys().Where(IsBindable).Where(k => keyState[k]).ToList();
        foreach (var k in down) captured.Add(k);

        if (down.Count > 0 || captured.Count == 0) return;

        var combo = captured.OrderBy(k => IsModifier(k) ? 0 : 1).ThenBy(k => (int)k).ToList();
        setTarget!(combo);
        configuration.Save();
        capturingLabel = null;
    }

    private static bool IsModifier(VirtualKey k) => k is VirtualKey.CONTROL or VirtualKey.MENU or VirtualKey.SHIFT;

    private static bool IsBindable(VirtualKey k) => k switch
    {
        VirtualKey.LBUTTON or VirtualKey.RBUTTON or VirtualKey.MBUTTON or VirtualKey.XBUTTON1 or VirtualKey.XBUTTON2 => false,
        VirtualKey.LSHIFT or VirtualKey.RSHIFT or VirtualKey.LCONTROL or VirtualKey.RCONTROL or VirtualKey.LMENU or VirtualKey.RMENU => false,
        VirtualKey.ESCAPE => false,
        _ => true,
    };
}
