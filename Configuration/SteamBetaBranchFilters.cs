using System;

namespace Tobey.Subnautica.ConfigHandler.Configuration;

[Flags]
internal enum SteamBetaBranchFilters
{
    None = 1 << 0,
    Legacy = 1 << 1,
    Experimental = 1 << 2,
    Any = None | Legacy | Experimental
}
