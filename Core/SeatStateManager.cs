using System.Collections.Generic;
using MCGalaxy;

namespace Seatify
{
    public static class SeatStateManager
    {
        public static HashSet<string> FrozenLevels = new HashSet<string>();
        static string file = "plugins/Seatify/freeze.txt";  
        static Dictionary<string, bool> sitting = new Dictionary<string, bool>();

        static Dictionary<string, string> frozenReason = new Dictionary<string, string>();

        static Dictionary<string, string> frozenBy = new Dictionary<string, string>();

        public static bool IsSitting(string player)
        {
            return sitting.ContainsKey(player) && sitting[player];
        }

        public static void SetSitting(string player, bool value)
        {
            sitting[player] = value;
        }

        public static void Remove(string player)
        {
            if (sitting.ContainsKey(player))
                sitting.Remove(player);
        }

        public static bool IsFrozen(string level)
        {
            return FrozenLevels.Contains(level.ToLower());
        }

        public static void Freeze(string level, string player = "Unknown", string reason = "No reason")
        {
            string key = level.ToLower();

            FrozenLevels.Add(key);
            frozenBy[key] = player;
            frozenReason[key] = reason;
            SaveFreeze();
        }

        public static void Unfreeze(string level)
        {
            string key = level.ToLower();

            FrozenLevels.Remove(key);
            frozenBy.Remove(key);
            frozenReason.Remove(key);
            SaveFreeze();
        }

        public static bool CanEdit(Player p)
        {
            string level = p.level.name.ToLower();

            if (!FrozenLevels.Contains(level))
                return true;

            string who = frozenBy.ContainsKey(level) ? frozenBy[level] : "Unknown";
            string reason = frozenReason.ContainsKey(level) ? frozenReason[level] : "No reason";

            p.Message("&cSeat editing is frozen here.");
            p.Message($"&7By: {who} | Reason: {reason}");
            return false;
        }

        public static void LoadFreeze()
        {
            FrozenLevels.Clear();
            frozenBy.Clear();
            frozenReason.Clear();

            if (!System.IO.File.Exists(file)) return;

            foreach (var line in System.IO.File.ReadAllLines(file))
            {
                var parts = line.Split(':');
                if (parts.Length < 1) continue;

                string level = parts[0].ToLower();
                FrozenLevels.Add(level);

                if (parts.Length >= 2)
                    frozenBy[level] = parts[1];

                if (parts.Length >= 3)
                    frozenReason[level] = parts[2];
            }
        }
        public static void SaveFreeze()
        {
            var lines = new List<string>();

            foreach (var level in FrozenLevels)
            {
                string by = frozenBy.ContainsKey(level) ? frozenBy[level] : "Unknown";
                string reason = frozenReason.ContainsKey(level) ? frozenReason[level] : "No reason";

                lines.Add(level + ":" + by + ":" + reason);
            }

            System.IO.File.WriteAllLines(file, lines);
        }

        public static string GetFrozenBy(string level)
        {
            level = level.ToLower();
            return frozenBy.ContainsKey(level) ? frozenBy[level] : "Unknown";
        }

        public static string GetFrozenReason(string level)
        {
            level = level.ToLower();
            return frozenReason.ContainsKey(level) ? frozenReason[level] : "No reason";
        }
    }
}   