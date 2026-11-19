using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;

namespace NoSkippy.Services;

public static class KeyCombo
{
    public static bool Held(IKeyState keyState, List<VirtualKey> keys)
    {
        return keys.Count > 0 && keys.All(k => keyState.IsVirtualKeyValid(k) && keyState[k]);
    }

    public static string Format(List<VirtualKey> keys)
    {
        if (keys.Count == 0) return "(none)";
        return string.Join("+", keys.Select(k => k switch
        {
            VirtualKey.CONTROL => "Ctrl",
            VirtualKey.MENU => "Alt",
            VirtualKey.SHIFT => "Shift",
            _ => k.ToString(),
        }));
    }
}
