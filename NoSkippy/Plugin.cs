using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using NoSkippy.Services;
using NoSkippy.Windows;

namespace NoSkippy;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInterop { get; private set; } = null!;
    [PluginService] internal static IKeyState KeyState { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDtrBar DtrBar { get; private set; } = null!;

    private const string CommandName = "/noskippy";

    private readonly TalkGuard talkGuard;
    private readonly SkipGuard skipGuard;
    private readonly PluginState state;
    private readonly Hotkeys hotkeys;
    private readonly DtrIndicator dtrIndicator;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("NoSkippy");
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Normalize();

        SpamLock.SelfCheck();
        state = new PluginState(Condition, ClientState, ChatGui, Configuration);
        talkGuard = new TalkGuard(AddonLifecycle, Configuration, state, Condition, GameGui, KeyState);
        skipGuard = new SkipGuard(AddonLifecycle, KeyState, ChatGui, GameInterop, Configuration, state);

        ConfigWindow = new ConfigWindow(this, KeyState);
        hotkeys = new Hotkeys(Framework, KeyState, Configuration, state, () => ConfigWindow.Capturing);
        dtrIndicator = new DtrIndicator(Framework, DtrBar, Configuration, state);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the No Skippy settings."
        });

        PluginInterface.UiBuilder.DisableCutsceneUiHide = true;
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.Draw += skipGuard.Draw;
        PluginInterface.UiBuilder.Draw += talkGuard.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.Draw -= skipGuard.Draw;
        PluginInterface.UiBuilder.Draw -= talkGuard.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        dtrIndicator.Dispose();
        hotkeys.Dispose();
        skipGuard.Dispose();
        talkGuard.Dispose();
        state.Dispose();
    }

    private void OnCommand(string command, string args) => ConfigWindow.Toggle();

    public void ToggleConfigUi() => ConfigWindow.Toggle();
}
