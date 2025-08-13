using System;
using Tobey.Subnautica.ConfigHandler.Configuration;

internal enum SteamBetaBranch
{
    None,
    legacy,
    experimental,
    march_2023,
}

internal static class SteamBetaBranchFiltersExtensions
{
    internal static SteamBetaBranchFilters AsFilter(this SteamBetaBranch value) =>
        (SteamBetaBranchFilters)Enum.Parse(typeof(SteamBetaBranchFilters), value.ToString());
}