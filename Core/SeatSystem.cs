using System;
using MCGalaxy;
using Seatify.Core;

namespace Seatify.Core
{
    public static class SeatSystem
    {
        static ushort SIT_BLOCK = 50;

        public static void UpdatePlayer(Player p)
        {
            if (p == null || p.Level == null) return;

            var pos = p.Pos.FeetBlockCoords;

            if (!p.Level.IsValidPos(pos.X, pos.Y, pos.Z)) return;

            ushort block = p.Level.GetBlock((ushort)pos.X, (ushort)pos.Y, (ushort)pos.Z);

            bool shouldSit = (block == SIT_BLOCK);
            bool isSitting = SeatStateManager.IsSitting(p.name);

            if (shouldSit)
            {
                if (isSitting) return;

                p.UpdateModel("sit");
                SeatStateManager.SetSitting(p.name, true);
            }
            else
            {
                if (!isSitting) return;

                p.UpdateModel("humanoid");
                SeatStateManager.SetSitting(p.name, false);
            }
        }
    }
}