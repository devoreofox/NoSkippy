using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace NoSkippy;

public static class ImGuiUtil
{
    public static void Tooltip(string text)
    {
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25f);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

    public static void DrawLock(ImDrawListPtr draw, Vector2 center, float scale, uint color = 0xFFE0E0E0)
    {
        var min = center + new Vector2(-6, -1) * scale;
        var max = center + new Vector2(6, 8) * scale;
        draw.AddRectFilled(min, max, color, 2 * scale);
        draw.AddRect(min, max, 0xFF202020, 2 * scale, ImDrawFlags.None, 1.5f * scale);
        draw.PathArcTo(center + new Vector2(0, -2) * scale, 4.5f * scale, MathF.PI, MathF.Tau, 12);
        draw.PathStroke(color, ImDrawFlags.None, 2.5f * scale);
    }
}
