using System;
using MCGalaxy;

namespace Seatify.Utils
{
    public static class BlockUtils
    {
        public static ushort GetBlockUnderPlayer(Player p)
        {
            var pos = p.Pos.FeetBlockCoords;

            if (!p.Level.IsValidPos(pos.X, pos.Y, pos.Z))
                return 0;

            return p.Level.GetBlock((ushort)pos.X, (ushort)pos.Y, (ushort)pos.Z);
        }

        public static bool IsValidPos(Player p)
        {
            var pos = p.Pos.FeetBlockCoords;
            return p.Level != null && p.Level.IsValidPos(pos.X, pos.Y, pos.Z);
        }
    }
}