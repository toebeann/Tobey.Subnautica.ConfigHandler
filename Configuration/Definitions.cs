using BepInEx;
using System.Text;

namespace Tobey.Subnautica.ConfigHandler.Configuration;
internal static class Definitions
{
    public static readonly Definition<bool> Enabled = new(
        Section: "General",
        Key: "Enabled",
        Description: "Whether the Config Handler patch should run on game launch.",
        DefaultValue: true,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });

    public static readonly Definition<bool> IgnoreUnsupportedGameWarning = new(
        Section: "General",
        Key: "Ignore unsupported game warning",
        Description: "Whether to ignore the unsupported game warning and apply configuration overrides anyway.",
        DefaultValue: false,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });

    public static readonly Definition<EntryPointOverrideMode> EntryPointOverrideMode = new(
        Section: "Override: Entry point",
        Key: "Mode",
        Description: new StringBuilder()
            .AppendLine("A value other than Filtered will cause the specified override to always be applied.")
            .Append("To disable the override, set to Filtered and set the branch filter to Disabled (uncheck all in Configuration Manager).")
            .ToString(),
        DefaultValue: Configuration.EntryPointOverrideMode.Filtered,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });

    public static readonly Definition<SteamBetaBranchFilters> QModManagerEntryPointBranchFilter = new(
        Section: "Override: Entry point",
        Key: "Branch filter",
        Description: "Which Steam beta branches should trigger the QModManager entry point to be applied in Automatic mode.",
        DefaultValue: Paths.ProcessName switch
        {
            "Subnautica" => SteamBetaBranchFilters.legacy,
            _ => SteamBetaBranchFilters.Any,
        },
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true, Order = -1 });

    public static readonly Definition<bool> IgnoreQModManager = new(
        Section: "Override: Entry point",
        Key: "Ignore QModManager",
        Description: "Whether to ignore the presence of QModManager and QMods when determining whether to apply the QModManager entry point in Automatic mode.",
        DefaultValue: false,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true, Order = -2 });

    public static readonly Definition<HideManagerGameObjectOverrideMode> HideManagerGameObjectOverrideMode = new(
        Section: "Override: HideManagerGameObject",
        Key: "Mode",
        Description: new StringBuilder()
            .AppendLine("A value other than Filtered will cause the specified override to always be applied.")
            .Append("To disable the override, set to Filtered and set the branch filter to Disabled (uncheck all in Configuration Manager).")
            .ToString(),
        DefaultValue: Configuration.HideManagerGameObjectOverrideMode.Filtered,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });

    public static readonly Definition<SteamBetaBranchFilters> HideManagerGameObjectBranchFilter = new(
        Section: "Override: HideManagerGameObject",
        Key: "Branch filter",
        Description: "Which Steam beta branches should trigger the HideManagerGameObject configuration to be applied in Automatic mode.",
        DefaultValue: Paths.ProcessName switch
        {
            "Subnautica" => SteamBetaBranchFilters.None | SteamBetaBranchFilters.experimental,
            _ => SteamBetaBranchFilters.Disabled,
        },
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true, Order = -1 });
}
