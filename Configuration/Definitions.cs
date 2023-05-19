using BepInEx;

namespace Tobey.Subnautica.ConfigHandler.Configuration;
internal static class Definitions
{
    public static readonly Definition<bool> Enabled = new(
        Section: "General",
        Key: "Enabled",
        Description: "Whether the Config Handler patch should run on game launch.",
        DefaultValue: true,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });

    public static readonly Definition<OverrideMode> OverrideMode = new(
        Section: "General",
        Key: "Configuration override mode",
        Description: "A value other than Automatic will cause the specified configuration override to always be applied.",
        DefaultValue: Configuration.OverrideMode.Automatic,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });

    public static readonly Definition<SteamBetaBranchFilters> SteamBetaBranchFilters = new(
        Section: "General",
        Key: "Steam beta branch filters",
        Description: "Which Steam beta branches should trigger the QModManager configuration to be applied in Automatic mode.",
        DefaultValue: Paths.ProcessName switch
        {
            "Subnautica" => Configuration.SteamBetaBranchFilters.Legacy,
            _ => Configuration.SteamBetaBranchFilters.Any
        },
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });

    public static readonly Definition<bool> IgnoreQModManager = new(
        Section: "General",
        Key: "Ignore QModManager",
        Description: "Whether to ignore the presence of QModManager and QMods when determining which configuration to apply in Automatic mode.",
        DefaultValue: false,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });

    public static readonly Definition<bool> IgnoreUnsupportedGameWarning = new(
        Section: "General",
        Key: "Ignore unsupported game warning",
        Description: "Whether to ignore the unsupported game warning and apply the configuration override anyway.",
        DefaultValue: false,
        Tags: new ConfigurationManagerAttributes { IsAdvanced = true });
}
