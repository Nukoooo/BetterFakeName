using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace BetterFakeName.Modules;

internal unsafe class TargetInfo(Configuration configuration) : IUiModule
{
    private TargetInfoConfig Config => configuration.TargetInfo;

    public bool Init()
    {
        /*DalamudApi.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_TargetInfo", OnTargetInfoPreDraw);*/
        DalamudApi.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, "_TargetInfoMainTarget", OnTargetInfoMainTargetPreUpdate);
        DalamudApi.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, "_FocusTargetInfo", OnFocusTargetPreUpdate);

        return true;
    }

    public void Shutdown()
    {
        /*DalamudApi.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "_TargetInfo", OnTargetInfoPreDraw);*/
        DalamudApi.AddonLifecycle.UnregisterListener(AddonEvent.PreUpdate, "_TargetInfoMainTarget", OnTargetInfoMainTargetPreUpdate);
        DalamudApi.AddonLifecycle.UnregisterListener(AddonEvent.PreUpdate, "_FocusTargetInfo", OnFocusTargetPreUpdate);
    }

    public string UiName => "目标信息";

    public void OnDrawUi()
    {
        var enabled = Config.Enabled;
        if (ImGui.Checkbox("启用##目标信息", ref enabled))
        {
            Config.Enabled = enabled;
            configuration.Save();
        }

        ImGui.BeginDisabled(!enabled);
        {
            var name = Config.Name;
            if (ImGui.InputTextWithHint("名字##目标信息", "", ref name, 32))
            {
                Config.Name = name;
                configuration.Save();
            }

            var companyTag = Config.FreeCompanyName;
            if (ImGui.InputTextWithHint("部队名字##目标信息", "", ref companyTag, 32))
            {
                Config.FreeCompanyName = companyTag;
                configuration.Save();
            }

            var focusTarget = Config.FocusTarget;
            if (ImGui.InputTextWithHint("焦点目标名字##目标信息", "", ref focusTarget, 32))
            {
                Config.FocusTarget = focusTarget;
                configuration.Save();
            }

            if (ImGui.Button("将目标名字应用到焦点目标上"))
            {
                Config.FocusTarget = Config.Name;
                configuration.Save();
            }
        }
        ImGui.EndDisabled();
    }

    /*private void OnTargetInfoPreDraw(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        var uldManager = addon->UldManager;
        if (uldManager.NodeListCount < 9)
            return;

        var nameNode = uldManager.NodeList[8];
        MemoryHelper.ReadStringNullTerminated((IntPtr)nameNode->GetAsAtkTextNode()->GetText(), out var str);
        if (!string.IsNullOrWhiteSpace(str))
        {
            DalamudApi.PluginLog.Info($"{str}");
        }
    }*/

    private void OnTargetInfoMainTargetPreUpdate(AddonEvent type, AddonArgs args)
    {
        if (DalamudApi.ClientState.LocalPlayer is not { } local)
            return;

        var addon = (AtkUnitBase*)args.Addon;
        var uldManager = addon->UldManager;
        if (!addon->IsVisible || uldManager.NodeListCount < 13)
            return;

        string? str;

        if (DalamudApi.TargetManager.Target is { } target && target.ObjectIndex == local.ObjectIndex)
        {
            var targetNameNode = uldManager.NodeList[8]->GetAsAtkTextNode();
            str = MemoryHelper.ReadStringNullTerminated((IntPtr) targetNameNode->GetText().Value);

            if (!string.IsNullOrWhiteSpace(str))
            {
                var name = Config.Enabled ? Config.Name : local.Name;
                var freeCompany = Config.Enabled ? Config.FreeCompanyName : local.CompanyTag.TextValue;

                var textToSet = name + " ";
                if (!string.IsNullOrWhiteSpace(freeCompany))
                    textToSet += $"«{freeCompany}»";

                targetNameNode->SetText(textToSet);
            }
        }

        var targetsTargetNameNode = uldManager.NodeList[12]->GetAsAtkTextNode();
        str = MemoryHelper.ReadStringNullTerminated((IntPtr) targetsTargetNameNode->GetText().Value);

        if (!string.IsNullOrWhiteSpace(str))
        {
            str = str.Replace(local.Name.TextValue, Config.Name);
            targetsTargetNameNode->SetText(str);
        }
    }

    private void OnFocusTargetPreUpdate(AddonEvent type, AddonArgs args)
    {
        if (DalamudApi.ClientState.LocalPlayer is not { } local || DalamudApi.TargetManager.FocusTarget is not { } target || target.ObjectIndex != local.ObjectIndex)
            return;

        var addon = (AtkUnitBase*)args.Addon;
        var uldManager = addon->UldManager;
        if (!addon->IsVisible || uldManager.NodeListCount < 13)
            return;

        var targetsTargetNameNode = uldManager.NodeList[10]->GetAsAtkTextNode();
        var str                   = MemoryHelper.ReadStringNullTerminated((IntPtr) targetsTargetNameNode->GetText().Value);

        if (!string.IsNullOrWhiteSpace(str))
        {
            var name = (Config.Enabled ? Config.FocusTarget : local.Name.TextValue);
            var whitespaceIndex = str.IndexOf(' ');
            if (whitespaceIndex >= 0)
                str = str[..(whitespaceIndex + 1)] + name;
            else
                str = name;
            
            targetsTargetNameNode->SetText(str);
        }
    }
}

public class TargetInfoConfig
{
    public bool Enabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FreeCompanyName { get; set; } = string.Empty;
    public string FocusTarget { get; set; } = string.Empty;
}