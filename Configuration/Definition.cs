namespace Tobey.Subnautica.ConfigHandler.Configuration;

internal record Definition<T>(string Section, string Key, string Description, T DefaultValue, params object[] Tags);
