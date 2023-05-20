using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Indieteur.VDFAPI;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private static readonly ManualLogSource logger = Logger.CreateLogSource("Legacy Config Handler");

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
                .GetProperty("CoreConfig", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) switch
            {
                ConfigFile coreConfig => coreConfig,
                _ => null
            };

            const string ENTRYPOINT_CONFIG_SECTION = "Preloader.Entrypoint";

            var entryPointAssembly = bepinexConfig?.GetConfigEntry<string>(ENTRYPOINT_CONFIG_SECTION, "Assembly");
            var entryPointType = bepinexConfig?.GetConfigEntry<string>(ENTRYPOINT_CONFIG_SECTION, "Type");
            var entryPointMethod = bepinexConfig?.GetConfigEntry<string>(ENTRYPOINT_CONFIG_SECTION, "Method");

            if (new[] { entryPointAssembly, entryPointMethod, entryPointType }.Contains(null))
            {
                logger.LogWarning("Failed to parse BepInEx config! Aborting patch.");
                return;
            }

            var overrideMode = config.Bind(Definitions.OverrideMode).Value switch
            {
                OverrideMode.Automatic when
                    config.Bind(Definitions.SteamBetaBranchFilters).Value.HasFlag(GetSteamBetaBranch().AsFilter()) &&
                    (config.Bind(Definitions.IgnoreQModManager).Value || HasEnabledQModManagerMods())
                    => OverrideMode.QModManager,
                OverrideMode.Automatic => OverrideMode.Default,
                OverrideMode mode => mode,
            };

            logger.LogInfo($"Applying override mode: {overrideMode}");

            switch (overrideMode)
            {
                case OverrideMode.QModManager:
                    entryPointAssembly.Value = "Assembly-CSharp.dll";
                    entryPointType.Value = "SystemsSpawner";
                    entryPointMethod.Value = "Awake";
                    break;

                default:
                    entryPointAssembly.ApplyDefaultValue();
                    entryPointType.ApplyDefaultValue();
                    entryPointMethod.ApplyDefaultValue();
                    break;
            }
        }

        private static SteamBetaBranch GetSteamBetaBranch()
        {
            try
            {
                var manifestPath = Path.GetFullPath(Path.Combine(Paths.GameRootPath, "../../", $"appmanifest_{SteamGameId}.acf"));
                return new VDFData(manifestPath)
                    .Nodes
                    .FindNode("AppState")
                    .Nodes
                    .FindNode("MountedConfig")
                    .Keys
                    .FindKey("BetaKey")?
                    .Value switch
                {
                    "legacy" => SteamBetaBranch.Legacy,
                    "experimental" => SteamBetaBranch.Experimental,
                    _ => SteamBetaBranch.None, // an empty or missing value means not on a beta branch
                };
            }
            catch
            {   // if we fail to parse the manifest (e.g. because it doesn't exist), we can assume the user is not on a beta branch
                return SteamBetaBranch.None;
            }
        }

        private static bool HasQModManager() =>
            Directory.GetFiles(Paths.PluginPath, "QModInstaller.dll", SearchOption.AllDirectories).Any(path =>
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

        private static bool HasEnabledQModManagerMods() =>
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
}
