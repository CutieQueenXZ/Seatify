using System;
using System.Collections.Generic;
using System.IO;
using MCGalaxy;
using System.Linq;

namespace Seatify
{
    public static class SeatManager
    {
        static HashSet<string> seats = new HashSet<string>();

        static string file = "plugins/Seatify/seats.txt";

        public static string Key(Level lvl, int x, int y, int z)
        {
            return lvl.name + ":" + x + ":" + y + ":" + z;
        }

        public static void Add(Level lvl, int x, int y, int z, string owner)
        {
            string key = lvl.name + ":" + x + ":" + y + ":" + z + ":" + owner;

            lock (seats)
            {
                seats.Add(key);
                Save();
            }
        }

        public static void Remove(Level lvl, int x, int y, int z)
        {
            string baseKey = Key(lvl, x, y, z);

            lock (seats)
            {
                seats.RemoveWhere(s => s.StartsWith(baseKey));
                Save();
            }
        }

        public static bool IsSeat(Level lvl, int x, int y, int z)
        {
            string baseKey = Key(lvl, x, y, z);

            lock (seats)
            {
                return seats.Any(s => s.StartsWith(baseKey));
            }
        }

        public static void Load()
        {
            seats.Clear();

            if (!File.Exists(file)) return;

            foreach (var line in File.ReadAllLines(file))
            {
                seats.Add(line.Trim());
            }
        }

        public static void Save()
        {
            lock (seats)
            {
                Directory.CreateDirectory("plugins/Seatify");
                File.WriteAllLines(file, seats);
            }
        }

        public static string[] GetAll()
        {
            lock (seats)
                return seats.ToArray();
        }

        public static void ClearAll()
        {
            seats.Clear();
            Save();
        }

        public static string GetOwner(Level lvl, int x, int y, int z)
        {
            foreach (var entry in GetAll())
            {
                var parts = entry.Split(':');
                if (parts.Length < 4) continue;

                string level = parts[0];

                if (!int.TryParse(parts[1], out int sx)) continue;
                if (!int.TryParse(parts[2], out int sy)) continue;
                if (!int.TryParse(parts[3], out int sz)) continue;

                if (level.CaselessEq(lvl.name) && sx == x && sy == y && sz == z)
                    return parts.Length >= 5 ? parts[4] : "Unknown";
            }
            return null;
        }
    }
}