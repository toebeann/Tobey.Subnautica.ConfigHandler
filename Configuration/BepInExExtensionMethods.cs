using BepInEx.Configuration;
using System.Linq;

namespace Tobey.Subnautica.ConfigHandler.Configuration;
internal static class BepInExExtensionMethods
{
    public static ConfigEntry<T> GetConfigEntry<T>(this ConfigFile configFile, string section, string key) =>
            configFile.FirstOrDefault(kvp =>
                kvp.Key.Section == section &&
                kvp.Key.Key == key)
            .Value switch
            {
                ConfigEntry<T> entry => entry,
                _ => null
            };

    public static ConfigEntry<T> Bind<T>(this ConfigFile configFile, Definition<T> args) => configFile.Bind(
        section: args.Section,
        key: args.Key,
        configDescription: new(
            description: args.Description,
            tags: args.Tags),
        defaultValue: args.DefaultValue);

    public static void ApplyDefaultValue<T>(this ConfigEntry<T> configEntry) => configEntry.Value = (T)configEntry.DefaultValue;
}
