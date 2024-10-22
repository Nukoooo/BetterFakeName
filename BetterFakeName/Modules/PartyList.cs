using Dalamud.Game.Addon.Lifecycle;
using System;
using System.Runtime.InteropServices;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

namespace BetterFakeName.Modules;

internal class PartyList(Configuration configuration) : IUiModule
{
    private PartyListConfig Config => configuration.PartyList;

    public bool Init()
    {
        DalamudApi.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_PartyList", OnPartyListPreDraw);

        return true;
    }

    public void Shutdown()
    {
        DalamudApi.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "_PartyList", OnPartyListPreDraw);
    }

    public string UiName => "小队列表";

    public void OnDrawUi()
    {
        var enabled = Config.Enabled;
        if (ImGui.Checkbox("启用##小队列表", ref enabled))
        {
            Config.Enabled = enabled;
            configuration.Save();
        }

        ImGui.BeginDisabled(!enabled);
        {
            var name = Config.Name;
            if (ImGui.InputTextWithHint("名字", "", ref name, 32))
            {
                Config.Name = name;
                configuration.Save();
            }
        }
        ImGui.EndDisabled();
    }

    private unsafe void OnPartyListPreDraw(AddonEvent type, AddonArgs args)
    {
        if (DalamudApi.ClientState.LocalPlayer is not { } local)
            return;
        
        var addon = (AddonPartyList*)args.Addon;
        var localMember = addon->PartyMembers[0];
        if (MemoryHelper.ReadStringNullTerminated((IntPtr)localMember.Name->GetText()) is not {} name)
            return;
        
        var whiteSpaceIndex = name.IndexOf(' ');
        if (whiteSpaceIndex != -1)
        {
            name = name[..(whiteSpaceIndex + 1)] + (Config.Enabled ? Config.Name : local.Name);
        }

        localMember.Name->SetText(name);
    }
}

public class PartyListConfig
{
    public bool Enabled { get; set; } = false;
    public string Name { get; set; } = string.Empty;
}
