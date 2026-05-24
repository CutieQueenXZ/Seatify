using System;
using MCGalaxy;

namespace Seatify.Utils
{
    public static class PlayerUtils
    {
        public static void SafeSetModel(Player p, string model)
        {
            if (p == null) return;
            if (p.Model == model) return;

            p.UpdateModel(model);
        }

        public static bool IsValid(Player p)
        {
            return p != null && p.Level != null;
        }
    }
}