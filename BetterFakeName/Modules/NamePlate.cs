using System.Collections.Generic;
using Dalamud.Hooking;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

namespace BetterFakeName.Modules;

internal class NamePlate(Configuration configuration) : IUiModule
{
    private NamePlateConfig Config => configuration.NamePlate;

    private Hook<AddonNamePlateSetPlayerNamePlateDelegate> AddonNamePlateHook { get; set; } = null!;

    public unsafe bool Init()
    {
        if (!DalamudApi.SigScanner.TryScanText("4C 8B DC 41 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 49 8B 40", out var address))
        {
            DalamudApi.PluginLog.Error("Failed to find address for AddonNamePlate.SetPlayerNamePlate");
            return false;
        }

        AddonNamePlateHook = DalamudApi.GameInterop.HookFromAddress<AddonNamePlateSetPlayerNamePlateDelegate>(address, hk_AddonNamePlateSetPlayerNamePlate);
        AddonNamePlateHook.Enable();
        return true;
    }

    public void Shutdown() => AddonNamePlateHook.Dispose();

    public string UiName => "铭牌";

    public void OnDrawUi()
    {
        var enabled = Config.Enabled;
        if (ImGui.Checkbox("启用铭牌更改", ref enabled))
        {
            Config.Enabled = enabled;
            configuration.Save();
        }

        ImGui.BeginDisabled(!enabled);
        {
            var name = Config.Name;
            if (ImGui.InputTextWithHint("铭牌名字", "", ref name, 32))
            {
                Config.Name = name;
                configuration.Save();
            }

            var freeCompanyName = Config.FreeCompanyName;
            if (ImGui.InputTextWithHint("部队名字", "", ref freeCompanyName, 32))
            {
                Config.FreeCompanyName = freeCompanyName;
                configuration.Save();
            }

            var battleNpc = Config.BattleNpcName;
            if (ImGui.InputTextWithHint("召唤物等名字", "", ref battleNpc, 32))
            {
                Config.BattleNpcName = battleNpc;
                configuration.Save();
            }

            var companionName = Config.CompanionName;
            if (ImGui.InputTextWithHint("宠物名字", "", ref companionName, 32))
            {
                Config.CompanionName = companionName;
                configuration.Save();
            }
        }
        ImGui.EndDisabled();
    }

    private unsafe void hk_AddonNamePlateSetPlayerNamePlate(AddonNamePlate* addon, nint a2, nint a3)
    {
        if (DalamudApi.ClientState.LocalPlayer is not { } local || !Config.Enabled)
        {
            AddonNamePlateHook.Original(addon, a2, a3);
            return;
        }

        var v4 = *(nint*)(a2 + 40);
        var count = (*(int**)(v4 + 32))[0];

        var v137 = *(nint*)(a3 + 32);
        var data = *(nint*)(v137 + 32);

        HashSet<(int idx, UIObjectKind kind, string name, string fcTag, string title)> originals = [];

        var v10 = 6;
        var localName = local.Name.TextValue;

        for (var i = 0; i < count; i++)
        {
            var namePtr = *(nint*)(data + 8 * i);
            var titleNamePtr = *(nint*)(data + 8 * i + 400);
            var fcNamePtr = *(nint*)(data + 8 * i + 800);
            /*var prefixPtr = *(nint*)(data + 8 * i + 1200);*/

            var uiObjectKind = *(UIObjectKind*)(*(nint*)(v4 + 32) + 4 * v10);

            v10 += 20;

            switch (uiObjectKind)
            {
                case UIObjectKind.PlayerCharacter:
                {
                    var originalName = MemoryHelper.ReadStringNullTerminated(namePtr);
                    if (!originalName.Equals(localName))
                        continue;
                    var originalFcTag = MemoryHelper.ReadStringNullTerminated(fcNamePtr);
                    originals.Add((i, uiObjectKind, originalName, originalFcTag, ""));

                    var fcNameToWrite = string.IsNullOrWhiteSpace(Config.FreeCompanyName) ? " " : $" «{Config.FreeCompanyName}»";

                    MemoryHelper.WriteString(namePtr, Config.Name);
                    MemoryHelper.WriteString(fcNamePtr, fcNameToWrite);
                    continue;
                }
                case UIObjectKind.EventNpcCompanion:
                case UIObjectKind.BattleNpcFriendly:
                {
                    var titleName = MemoryHelper.ReadStringNullTerminated(titleNamePtr);
                    if (string.IsNullOrWhiteSpace(titleName))
                        continue;

                    var ownerName = titleName[1..^1];
                    if (!ownerName.Equals(localName))
                        continue;
                    originals.Add((i, uiObjectKind, "", "", titleName));

                    var nameToReplace = uiObjectKind == UIObjectKind.EventNpcCompanion ? Config.CompanionName : Config.BattleNpcName;
                    var nameToWrite = string.IsNullOrWhiteSpace(nameToReplace) ? " " : titleName[0] + nameToReplace + titleName[^1];

                    MemoryHelper.WriteString(titleNamePtr, nameToWrite);
                    continue;
                }
            }
        }

        AddonNamePlateHook.Original(addon, a2, a3);

        foreach (var v in originals)
        {
            switch (v.kind)
            {
                case UIObjectKind.PlayerCharacter:
                    MemoryHelper.WriteString(*(nint*)(data + 8 * v.idx), v.name);
                    MemoryHelper.WriteString(*(nint*)(data + 8 * v.idx + 800), v.fcTag);
                    continue;
                case UIObjectKind.BattleNpcFriendly or UIObjectKind.EventNpcCompanion:
                    MemoryHelper.WriteString(*(nint*)(data + 8 * v.idx + 400), v.title);
                    break;
            }
        }
    }

    private unsafe delegate void AddonNamePlateSetPlayerNamePlateDelegate(AddonNamePlate* addonNamePlate, nint a2, nint a3);
}

public class NamePlateConfig
{
    public bool Enabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FreeCompanyName { get; set; } = string.Empty;
    public string CompanionName { get; set; } = string.Empty;
    public string BattleNpcName { get; set; } = string.Empty;
}