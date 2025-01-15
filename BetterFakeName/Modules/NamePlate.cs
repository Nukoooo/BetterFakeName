using System.Collections.Generic;
using Dalamud.Game.Gui.NamePlate;
using ImGuiNET;

namespace BetterFakeName.Modules;

internal class NamePlate(Configuration configuration) : IUiModule
{
    private NamePlateConfig Config => configuration.NamePlate;

    public bool Init()
    {
        DalamudApi.NamePlate.OnDataUpdate += OnDataUpdate;

        return true;
    }

    private void OnDataUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        if (!Config.Enabled || DalamudApi.ClientState.LocalPlayer is not { } local)
        {
            return;
        }

        foreach (var handler in handlers)
        {
            switch (handler.NamePlateKind)
            {
                case NamePlateKind.PlayerCharacter:
                {
                    if (handler.GameObjectId != local.GameObjectId)
                    {
                        continue;
                    }

                    handler.SetField(NamePlateStringField.Name,           Config.Name);
                    handler.SetField(NamePlateStringField.FreeCompanyTag, Config.FreeCompanyName);

                    break;
                }

                case NamePlateKind.EventNpcCompanion:
                case NamePlateKind.BattleNpcFriendly:
                {
                    if (string.IsNullOrWhiteSpace(handler.Title.TextValue))
                    {
                        continue;
                    }

                    var isCompanion = handler.NamePlateKind == NamePlateKind.EventNpcCompanion;

                    var offset = isCompanion ? 1 : 2;

                    if (handler.GameObject is not { } obj
                        || DalamudApi.ObjectTable[obj.ObjectIndex - offset] is not { } owner
                        || owner.EntityId != local.EntityId)
                    {
                        continue;
                    }

                    var nameToReplace = isCompanion
                        ? Config.CompanionName
                        : Config.BattleNpcName;

                    handler.SetField(NamePlateStringField.Title, " «" + nameToReplace + "»");

                    break;
                }
            }
        }
    }

    public void Shutdown()
    {
        DalamudApi.NamePlate.OnDataUpdate -= OnDataUpdate;
    }

    public string UiName => "铭牌";

    public void OnDrawUi()
    {
        var enabled = Config.Enabled;

        if (ImGui.Checkbox("启用铭牌更改", ref enabled))
        {
            Config.Enabled = enabled;
            DalamudApi.NamePlate.RequestRedraw();
            configuration.Save();
        }

        ImGui.BeginDisabled(!enabled);

        {
            var name = Config.Name;

            if (ImGui.InputTextWithHint("铭牌名字", "", ref name, 32))
            {
                Config.Name = name;
                DalamudApi.NamePlate.RequestRedraw();
                configuration.Save();
            }

            var freeCompanyName = Config.FreeCompanyName;

            if (ImGui.InputTextWithHint("部队名字", "", ref freeCompanyName, 32))
            {
                Config.FreeCompanyName = freeCompanyName;
                DalamudApi.NamePlate.RequestRedraw();
                configuration.Save();
            }

            var battleNpc = Config.BattleNpcName;

            if (ImGui.InputTextWithHint("召唤物等名字", "", ref battleNpc, 32))
            {
                Config.BattleNpcName = battleNpc;
                DalamudApi.NamePlate.RequestRedraw();
                configuration.Save();
            }

            var companionName = Config.CompanionName;

            if (ImGui.InputTextWithHint("宠物名字", "", ref companionName, 32))
            {
                Config.CompanionName = companionName;
                DalamudApi.NamePlate.RequestRedraw();
                configuration.Save();
            }
        }

        ImGui.EndDisabled();
    }
}

public class NamePlateConfig
{
    public bool   Enabled         { get; set; }
    public string Name            { get; set; } = string.Empty;
    public string FreeCompanyName { get; set; } = string.Empty;
    public string CompanionName   { get; set; } = string.Empty;
    public string BattleNpcName   { get; set; } = string.Empty;
}
