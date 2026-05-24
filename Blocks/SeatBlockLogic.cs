using System;
using MCGalaxy;
using Seatify.Config;
using Seatify.Core;

namespace Seatify.Blocks
{
    public static class SeatBlockLogic
    {
        // called when checking a block under player
        public static bool IsSitBlock(Level lvl, Position pos)
        {
            if (!lvl.IsValidPos(pos.X, pos.Y, pos.Z)) return false;

            ushort block = lvl.GetBlock((ushort)pos.X, (ushort)pos.Y, (ushort)pos.Z);

            return block == SeatConfig.SitBlock;
        }
    }
}