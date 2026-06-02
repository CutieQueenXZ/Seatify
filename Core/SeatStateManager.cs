using System.Collections.Generic;
using System.IO;
using MCGalaxy;

namespace Seatify
{
    public static class SeatStateManager
    {   
        static object locker = new object(); 
        public static HashSet<string> FrozenLevels = new HashSet<string>();
        static string file = "plugins/Seatify/freeze.txt";  
        static HashSet<string> sitting = new HashSet<string>();

        static Dictionary<string, string> frozenReason = new Dictionary<string, string>();

        static Dictionary<string, string> frozenBy = new Dictionary<string, string>();

        public static HashSet<string> Active = new HashSet<string>();

        public static bool IsSitting(string player)
        {
            lock (locker)
                return sitting.Contains(player);
        }

        public static void SetSitting(string player, bool value)
        {
            lock (locker)
            {
                if (value) sitting.Add(player);
                else sitting.Remove(player);
            }
        }

        public static void Remove(string player)
        {
            lock (locker)
                sitting.Remove(player);
        }

        public static bool IsFrozen(string level)
        {
            lock (locker)
                return FrozenLevels.Contains(level.ToLower());
        }

        public static void Freeze(string level, string player = "Unknown", string reason = "No reason")
        {
            string key = level.ToLower();

            lock (locker)
            {
                FrozenLevels.Add(key);
                frozenBy[key] = player;
                frozenReason[key] = reason;
                SaveFreeze();
            }
        }

        public static void Unfreeze(string level)
        {
            string key = level.ToLower();

            lock (locker)
            {
                FrozenLevels.Remove(key);
                frozenBy.Remove(key);
                frozenReason.Remove(key);
                SaveFreeze();
            }
        }

        public static bool CanEdit(Player p)
        {
            string level = p.level.name.ToLower();

            lock (locker)
            {
                if (!FrozenLevels.Contains(level))
                    return true;

                frozenBy.TryGetValue(level, out var who);
                frozenReason.TryGetValue(level, out var reason);

                who ??= "Unknown";
                reason ??= "No reason";

                p.Message("&cSeat editing is frozen here.");
                p.Message($"&7By: {who} | Reason: {reason}");
                return false;
            }
        }

        public static void LoadFreeze()
        {
            lock (locker)
            {
                FrozenLevels.Clear();
                frozenBy.Clear();
                frozenReason.Clear();

                if (!File.Exists(file)) return;

                foreach (var line in File.ReadAllLines(file))
                {
                    var parts = line.Split(new[] { ':' }, 3);
                    if (parts.Length < 1) continue;

                    string level = parts[0].ToLower();
                    FrozenLevels.Add(level);

                    if (parts.Length >= 2)
                        frozenBy[level] = parts[1];

                    if (parts.Length >= 3)
                        frozenReason[level] = parts[2];
                }
            }
        }

        public static void SaveFreeze()
        {
            List<string> lines;

            lock (locker)
            {
                lines = new List<string>();

                foreach (var level in FrozenLevels)
                {
                    string by = frozenBy.TryGetValue(level, out var b) ? b : "Unknown";
                    string reason = frozenReason.TryGetValue(level, out var r) ? r : "No reason";

                    lines.Add(level + ":" + by + ":" + reason);
                }

                File.WriteAllLines(file, lines);
            }
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