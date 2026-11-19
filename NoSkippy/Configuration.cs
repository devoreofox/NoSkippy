using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Newtonsoft.Json;

namespace NoSkippy;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool TalkLockEnabled { get; set; } = true;
    public int RapidClickMs { get; set; } = 600;
    public int LockReleaseMs { get; set; } = 700;
    public int TalkDelayMs { get; set; } = 500;
    public bool SkipComboBypassesTalk { get; set; } = true;
    public bool WaitForText { get; set; } = true;
    public bool TalkLockInCutscenes { get; set; } = true;
    public bool TalkLockOutsideCutscenes { get; set; } = true;
    public bool ShowTalkLock { get; set; } = true;

    public bool SkipGuardEnabled { get; set; } = true;
    public bool NeverAllowSkip { get; set; } = false;
    public int SkipHoldMs { get; set; } = 1000;
    public float RingScale { get; set; } = 1.8f;
    public int RingOffsetX { get; set; } = 28;
    public int RingOffsetY { get; set; } = -28;
    public string SkipPromptText { get; set; } = "Skip cutscene?";
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<VirtualKey> SkipComboKeys { get; set; } = new() { VirtualKey.CONTROL, VirtualKey.SHIFT };

    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<VirtualKey> PauseKeys { get; set; } = new() { VirtualKey.CONTROL, VirtualKey.MENU, VirtualKey.P };
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<VirtualKey> MasterOffKeys { get; set; } = new() { VirtualKey.CONTROL, VirtualKey.MENU, VirtualKey.O };
    public int PauseMinutes { get; set; } = 2;
    public bool ChatMessages { get; set; } = true;
    public bool ShowDtr { get; set; } = true;

    public void Normalize()
    {
        SkipComboKeys = SkipComboKeys.Distinct().ToList();
        PauseKeys = PauseKeys.Distinct().ToList();
        MasterOffKeys = MasterOffKeys.Distinct().ToList();
    }

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
