using BepInEx;
using Tobey.Subnautica.ConfigHandler.Configuration;

namespace Tobey.Subnautica.ConfigHandler;
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Subnautica"), BepInProcess("SubnauticaZero"), BepInProcess("Subnautica Below Zero")]
public class Plugin : BaseUnityPlugin
{
    // this plugin only exists to allow you to edit the patcher config in-game via e.g. Configuration Manager
    private void Awake()
    {
        Config.Bind(Definitions.Enabled);
        Config.Bind(Definitions.OverrideMode);
        Config.Bind(Definitions.SteamBetaBranchFilters);
        Config.Bind(Definitions.IgnoreQModManager);
        Config.Bind(Definitions.IgnoreUnsupportedGameWarning);
    }
}
