using System;

namespace Tobey.Subnautica.ConfigHandler.Configuration;

[Flags]
internal enum SteamBetaBranchFilters
{
    Disabled = 0,
    None = 1 << 0,
    legacy = 1 << 1,
    experimental = 1 << 2,
    march_2023 = 1 << 3,
    Any = None | legacy | march_2023 | experimental
}
