using System;
using System.Collections.Generic;
using System.IO;
using MCGalaxy;

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
            seats.Add(key);
            Save();
        }
        public static void Remove(Level lvl, int x, int y, int z)
        {
            string baseKey = Key(lvl, x, y, z);

            seats.RemoveWhere(s => s.StartsWith(baseKey));
            Save();
        }

        public static bool IsSeat(Level lvl, int x, int y, int z)
        {
            string baseKey = Key(lvl, x, y, z);

            foreach (var s in seats)
            {
                if (s.StartsWith(baseKey)) return true;
            }

            return false;
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
            Directory.CreateDirectory("plugins/Seatify");

            File.WriteAllLines(file, seats);
        }

        public static IEnumerable<string> GetAll()
        {
            return seats;
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
                int sx = int.Parse(parts[1]);
                int sy = int.Parse(parts[2]);
                int sz = int.Parse(parts[3]);

                if (level.CaselessEq(lvl.name) && sx == x && sy == y && sz == z)
                    return parts.Length >= 5 ? parts[4] : "Unknown";
            }
            return null;
        }
    }
}