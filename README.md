# Tobey's Subnautica Config Handler

A BepInEx patcher for Subnautica and Subnautica: Below Zero to automatically handle configuration of important BepInEx config entries, setting them as needed depending on the user's Steam beta branch.

Includes a BepInEx plugin for in-game configuration with [Configuration Manager](https://github.com/toebeann/BepInEx.ConfigurationManager.Subnautica) (requires enabling "Advanced settings").

## Usage

Generally speaking, you shouldn't need to do much beyond plopping the contents of the downloaded .zip from [the releases page](https://github.com/toebeann/Tobey.Subnautica.ConfigHandler/releases) into your game folder (after installing BepInEx, of course).

However, if manual configuration is desired, there are several configuration options which can be edited in-game with [Configuration Manager](https://github.com/toebeann/BepInEx.ConfigurationManager.Subnautica) (requires enabling "Advanced settings"), or by editing the `Tobey.Subnautica.ConfigHandler.cfg` file generated in `BepInEx\config`:

```
## Settings file was created by plugin Subnautica Config Handler v1.1.0
## Plugin GUID: Tobey.Subnautica.ConfigHandler

[General]

## Whether the Config Handler patch should run on game launch.
# Setting type: Boolean
# Default value: true
Enabled = true

## Whether to ignore the unsupported game warning and apply configuration overrides anyway.
# Setting type: Boolean
# Default value: false
Ignore unsupported game warning = false

[Override: Entry point]

## A value other than Filtered will cause the specified override to always be applied.
## To disable the override, set to Filtered and set the branch filter to Disabled (uncheck all in Configuration Manager).
# Setting type: EntryPointOverrideMode
# Default value: Filtered
# Acceptable values: Filtered, Default, QModManager
Mode = Filtered

## Which Steam beta branches should trigger the QModManager entry point to be applied in Automatic mode.
# Setting type: SteamBetaBranchFilters
# Default value: legacy
# Acceptable values: Disabled, None, legacy, experimental, march_2023, Any
# Multiple values can be set at the same time by separating them with , (e.g. Debug, Warning)
Branch filter = legacy

## Whether to ignore the presence of QModManager and QMods when determining whether to apply the QModManager entry point in Automatic mode.
# Setting type: Boolean
# Default value: false
Ignore QModManager = false

[Override: HideManagerGameObject]

## A value other than Filtered will cause the specified override to always be applied.
## To disable the override, set to Filtered and set the branch filter to Disabled (uncheck all in Configuration Manager).
# Setting type: HideManagerGameObjectOverrideMode
# Default value: Filtered
# Acceptable values: Filtered, True, False
Mode = Filtered

## Which Steam beta branches should trigger the HideManagerGameObject configuration to be applied in Automatic mode.
# Setting type: SteamBetaBranchFilters
# Default value: None, experimental
# Acceptable values: Disabled, None, legacy, experimental, march_2023, Any
# Multiple values can be set at the same time by separating them with , (e.g. Debug, Warning)
Branch filter = None, experimental
```
