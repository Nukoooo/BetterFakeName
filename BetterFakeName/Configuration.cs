using BetterFakeName.Modules;
using Dalamud.Configuration;

namespace BetterFakeName;

internal class Configuration : IPluginConfiguration
{
    public NamePlateConfig NamePlate { get; set; } = new();
    public PartyListConfig PartyList { get; set; } = new();
    public TargetInfoConfig TargetInfo { get; set; } = new();

    int IPluginConfiguration.Version { get; set; }

    public void Save() => DalamudApi.Interface.SavePluginConfig(this);
}