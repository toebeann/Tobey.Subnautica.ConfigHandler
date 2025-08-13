using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Indieteur.VDFAPI;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Tobey.Subnautica.ConfigHandler.Configuration;

namespace Tobey.Subnautica.ConfigHandler
{
    public static class Patcher
    {

        // Without the contents of this region, the patcher will not be loaded by BepInEx - do not remove!
        #region BepInEx Patcher Contract
        public static IEnumerable<string> TargetDLLs { get; } = Enumerable.Empty<string>();
        public static void Patch(AssemblyDefinition _) { }
        #endregion

        private static readonly ManualLogSource logger = Logger.CreateLogSource("Branch Config Handler");

        private static RunningGame RunningGame => Paths.ProcessName switch
        {
            "Subnautica" => RunningGame.Subnautica,
            "SubnauticaZero" or "Subnautica Below Zero" => RunningGame.SubnauticaBelowZero,
            _ => RunningGame.Unsupported,
        };

        private static string SteamGameId = RunningGame switch
        {
            RunningGame.Subnautica => "264710",
            RunningGame.SubnauticaBelowZero => "848450",
            _ => null,
        };

        // this is our patch entry point
        public static void Initialize()
        {
            ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, "Tobey.Subnautica.ConfigHandler.cfg"), true);
            if (!config.Bind(Definitions.Enabled).Value)
            {
                logger.LogInfo("Config Handler patch disabled.");
                return;
            }

            if (RunningGame == RunningGame.Unsupported)
            {
                if (!config.Bind(Definitions.IgnoreUnsupportedGameWarning).Value)
                {
                    logger.LogWarning("We appear to be running on an unsupported game! Aborting patch.");
                    logger.LogMessage("If you would like to ignore this warning and apply the config override anyway, set the \"Ignore unsupported game warning\" config option to true.");
                    return;
                }
                else
                {
                    logger.LogWarning("We appear to be running on an unsupported game, but the \"Ignore unsupported game warning\" config option is set to true. Continuing patch.");
                }
            }

            ConfigFile bepinexConfig = typeof(ConfigFile)
                .GetProperty("CoreConfig", BindingFlags.Static | BindingFlags.NonPublic)?
                .GetValue(null) as ConfigFile;

            const string ENTRYPOINT_CONFIG_SECTION = "Preloader.Entrypoint";
            var entryPointAssembly = bepinexConfig?.GetEntry<string>(ENTRYPOINT_CONFIG_SECTION, "Assembly");
            var entryPointType = bepinexConfig?.GetEntry<string>(ENTRYPOINT_CONFIG_SECTION, "Type");
            var entryPointMethod = bepinexConfig?.GetEntry<string>(ENTRYPOINT_CONFIG_SECTION, "Method");

            if (new dynamic[] { entryPointAssembly, entryPointMethod, entryPointType }.Contains(null))
            {
                logger.LogWarning("Failed to parse BepInEx config! Aborting patch.");
                return;
            }

            var branch = GetSteamBetaBranch();
            logger.LogInfo($"Detected branch: {branch switch
            {
                SteamBetaBranch.None when !File.Exists(manifestPath) => "Non-Steam install, treating as None",
                _ => branch
            }}");

            // determine entry point override mode
            var qmodManagerEntryPointBranchFilter = config.Bind(Definitions.QModManagerEntryPointBranchFilter);

            var entryPointOverrideMode = config.Bind(Definitions.EntryPointOverrideMode).Value switch
            {
                EntryPointOverrideMode.Filtered when
                    qmodManagerEntryPointBranchFilter.Value.HasFlag(branch.AsFilter()) &&
                    (config.Bind(Definitions.IgnoreQModManager).Value || HasEnabledQModManagerMods())
                        => EntryPointOverrideMode.QModManager,

                EntryPointOverrideMode.Filtered when
                    qmodManagerEntryPointBranchFilter.Value != SteamBetaBranchFilters.Disabled
                        => EntryPointOverrideMode.Default,

                EntryPointOverrideMode mode => mode,
            };

            // if still filtered then we're skipping the override
            if (entryPointOverrideMode == EntryPointOverrideMode.Filtered)
            {
                logger.LogInfo("Entry point override disabled by branch filters");
            }
            else
            {
                logger.LogInfo($"Applying entry point override: {entryPointOverrideMode}");

                switch (entryPointOverrideMode)
                {
                    case EntryPointOverrideMode.QModManager:
                        entryPointAssembly.Value = "Assembly-CSharp.dll";
                        entryPointType.Value = "SystemsSpawner";
                        entryPointMethod.Value = "Awake";
                        break;

                    case EntryPointOverrideMode.Default:
                        entryPointAssembly.ApplyDefaultValue();
                        entryPointType.ApplyDefaultValue();
                        entryPointMethod.ApplyDefaultValue();
                        break;
                }
            }

            // determine chainloader override mode
            var hideManagerGameObjectBranchFilter = config.Bind(Definitions.HideManagerGameObjectBranchFilter);

            var chainloaderOverrideMode = config.Bind(Definitions.HideManagerGameObjectOverrideMode).Value switch
            {
                HideManagerGameObjectOverrideMode.Filtered when
                    hideManagerGameObjectBranchFilter.Value.HasFlag(branch.AsFilter())
                        => HideManagerGameObjectOverrideMode.True,

                HideManagerGameObjectOverrideMode.Filtered when
                    hideManagerGameObjectBranchFilter.Value != SteamBetaBranchFilters.Disabled
                        => HideManagerGameObjectOverrideMode.False,

                HideManagerGameObjectOverrideMode mode => mode,
            };

            // if still filtered then we're skipping
            if (chainloaderOverrideMode == HideManagerGameObjectOverrideMode.Filtered)
            {
                logger.LogInfo("HideManagerGameObject override disabled by branch filters");
            }
            else
            {
                logger.LogInfo($"Applying HideManagerGameObject override: {chainloaderOverrideMode}");

                switch (chainloaderOverrideMode)
                {
                    case HideManagerGameObjectOverrideMode.True:
                        bepinexConfig.SetHideManagerGameObject(true);
                        break;

                    case HideManagerGameObjectOverrideMode.False:
                        bepinexConfig.SetHideManagerGameObject(false);
                        break;
                }
            }
        }

        public static ConfigEntry<T> GetEntry<T>(this ConfigFile configFile, string section, string key)
            => configFile?[section, key] as ConfigEntry<T>;

        public static void ApplyDefaultValue<T>(this ConfigEntry<T> configEntry) => configEntry.Value = (T)configEntry.DefaultValue;

        private static void SetHideManagerGameObject(this ConfigFile configFile, bool value)
        {
            const string SECTION = "Chainloader";
            const string KEY = "HideManagerGameObject";

            try
            {
                configFile.GetEntry<bool>(SECTION, KEY).Value = value;
            }
            catch
            {
                configFile.Bind(SECTION, KEY, false,
                    description: new StringBuilder()
                        .AppendLine("If enabled, hides BepInEx Manager GameObject from Unity.")
                        .AppendLine("This can fix loading issues in some games that attempt to prevent BepInEx from being loaded.")
                        .AppendLine("Use this only if you know what this option means, as it can affect functionality of some older plugins.")
                        .ToString()).Value = value;
            }
        }

        private static readonly string manifestPath = Path.GetFullPath(Path.Combine(Paths.GameRootPath, "../../", $"appmanifest_{SteamGameId}.acf"));
        private static SteamBetaBranch GetSteamBetaBranch()
        {
            try
            {
                return new VDFData(manifestPath)
                    .Nodes
                    .FindNode("AppState")
                    .Nodes
                    .FindNode("MountedConfig")
                    .Keys
                    .FindKey("BetaKey")?
                    .Value.ToLowerInvariant() switch
                {
                    string betaKey => Enum.TryParse<SteamBetaBranch>(betaKey, out var branch)
                        ? branch
                        : SteamBetaBranch.None,
                    _ => SteamBetaBranch.None,
                };
            }
            catch
            {   // if we fail to parse the manifest (e.g. because it doesn't exist), we can assume the user is not on a beta branch
                return SteamBetaBranch.None;
            }
        }

        private static bool HasQModManager()
        {
            try
            {
                return
                    Directory.GetFiles(Paths.PluginPath, "QModInstaller.dll", SearchOption.AllDirectories)
                    .Any(path =>
                    {
                        try
                        {
                            using var assembly = AssemblyDefinition.ReadAssembly(path);
                            var types = assembly.Modules.SelectMany(module => module.GetAllTypes());
                            return types.Any(type =>
                                type.CustomAttributes.Any(attribute =>
                                    attribute.AttributeType.FullName == typeof(BepInPlugin).FullName &&
                                    attribute.ConstructorArguments.FirstOrDefault().Value is string pluginGuid &&
                                    pluginGuid == "QModManager.QMMLoader"));
                        }
                        catch
                        {   // if we fail to parse an assembly, it's probably not gonna be QMM...
                            return false;
                        }
                    });
            }
            catch
            {   // if we got here it probably means that Paths.PluginPath does not refer to a directory which exists
                // i.e. QMM definitely isn't installed...
                return false;
            }
        }

        private static bool HasEnabledQModManagerMods()
        {
            try
            {
                return
                    HasQModManager() &&
                    Directory.GetDirectories(Path.Combine(Paths.GameRootPath, "QMods"))
                    .Select(dir => Path.Combine(dir, "mod.json"))
                    .Any(path =>
                {
                    try
                    {
                        return
                            File.Exists(path) &&
                            (JsonConvert.DeserializeObject<JToken>(File.ReadAllText(path)) as JObject).TryGetValue("Enable", out var key) &&
                            key.Type == JTokenType.Boolean &&
                            key.Value<bool>();
                    }
                    catch (FileNotFoundException e)
                    when (e.FileName == path)
                    {   // seems the mod.json disappeared somehow, just ignore it as if there's no mod.json then qmm won't load it
                        return false;
                    }
                    catch (FileNotFoundException)
                    {   // when e.FileName != path this is usually an error about a needed assembly not being found by the runtime.
                        // most likely cause of this is user is on SN1 legacy branch without corlibs package,
                        // i.e. System.Runtime.Serialization assembly is missing, which our embedded copy of Json.NET depends upon.
                        // so, this means we can't parse JSON without some other means, and it's not worth it to keep trying,
                        // so let's short-circuit the iterator by returning true, skipping the check for enabled QMods
                        return true;
                    }
                    catch
                    {   // if we fail to parse the mod.json for any other reason, we'll just assume it's not an enabled qmod
                        // e.g. because it's not valid JSON
                        return false;
                    }
                });
            }
            catch
            {   // if we got here it probably means that the QMods folder does not exist, i.e. QMods are def. not installed
                return false;
            }
        }
    }
}
