# Tobey's Subnautica Config Handler

A BepInEx patcher for Subnautica and Subnautica: Below Zero to automatically handle common BepInEx configuration entries - particularly useful for transitioning to and from the legacy branch to use QModManager.

Includes a BepInEx plugin for in-game configuraiton with [Configuration Manager](https://github.com/toebeann/BepInEx.ConfigurationManager.Subnautica) (requires enabling "Advanced settings").

## Usage

Generally speaking you shouldn't need to do much beyond plopping the contents of the downloaded .zip from the releases page into your game folder (after installing BepInEx, of course).

However, if configuration is required, there are several configuration options, which can be edited in-game with [Configuration Manager](https://github.com/toebeann/BepInEx.ConfigurationManager.Subnautica) (requires enabling "Advanced settings"), or by editing the `Tobey.Subnautica.ConfigHandler.cfg` file generated in `BepInEx\config`:

```
## Settings file was created by plugin Subnautica Config Handler v1.0.2
## Plugin GUID: Tobey.Subnautica.ConfigHandler

[General]

## Whether the Config Handler patch should run on game launch.
# Setting type: Boolean
# Default value: true
Enabled = true

## A value other than Automatic will cause the specified configuration override to always be applied.
# Setting type: OverrideMode
# Default value: Automatic
# Acceptable values: Automatic, Default, QModManager
Configuration override mode = Automatic

## Which Steam beta branches should trigger the QModManager configuration to be applied in Automatic mode.
# Setting type: SteamBetaBranchFilters
# Default value: Legacy
# Acceptable values: None, Legacy, Experimental, Any
# Multiple values can be set at the same time by separating them with , (e.g. Debug, Warning)
Steam beta branch filters = Legacy

## Whether to ignore the presence of QModManager and QMods when determining which configuration to apply in Automatic mode.
# Setting type: Boolean
# Default value: false
Ignore QModManager = false

## Whether to ignore the unsupported game warning and apply the configuration override anyway.
# Setting type: Boolean
# Default value: false
Ignore unsupported game warning = false
```
