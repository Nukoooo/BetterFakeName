﻿using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BetterFakeName;

internal class DalamudApi
{
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    [PluginService] internal static IDalamudPluginInterface Interface { get; private set; } = null!;

    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;

    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;

    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService] internal static IFateTable FateTable { get; private set; } = null!;

    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;

    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    [PluginService] internal static ICondition Condition { get; private set; } = null!;

    [PluginService] internal static IPartyList PartyList { get; private set; } = null!;

    [PluginService] internal static IGameInteropProvider GameInterop { get; private set; } = null!;

    [PluginService] internal static IGameNetwork GameNetwork { get; private set; } = null!;

    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;

    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;

    [PluginService]
    internal static INamePlateGui NamePlate { get; private set; } = null!;
}