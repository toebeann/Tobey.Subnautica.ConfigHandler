using BepInEx.Configuration;

namespace Tobey.Subnautica.ConfigHandler.Configuration;
internal static class BepInExExtensionMethods
{
    public static ConfigEntry<T> Bind<T>(this ConfigFile configFile, Definition<T> args) => configFile.Bind(
            section: args.Section,
            key: args.Key,
            configDescription: new(
                description: args.Description,
                tags: args.Tags),
            defaultValue: args.DefaultValue);
}
