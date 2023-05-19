using System;
using Tobey.Subnautica.ConfigHandler.Configuration;

internal enum SteamBetaBranch
{
    None,
    Legacy,
    Experimental,
}

internal static class SteamBetaBranchFiltersExtensions
{
    internal static SteamBetaBranchFilters AsFilter(this SteamBetaBranch value) =>
        (SteamBetaBranchFilters)Enum.Parse(typeof(SteamBetaBranchFilters), value.ToString());
}