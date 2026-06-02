using System.Collections.Generic;
using BlockID = System.UInt16;
using MCGalaxy.Drawing;
using MCGalaxy.Maths;
using MCGalaxy;
using System;

namespace Seatify
{
    public class CmdSeat : Command
    {
        public override string name => "Seat";
        public override string type => "Seatify";

        public override LevelPermission defaultRank => LevelPermission.AdvBuilder;

        public static HashSet<string> AddClickMode = new HashSet<string>();
        public static HashSet<string> RemoveClickMode = new HashSet<string>();

        static bool CanModifySeat(Player p, Level lvl, int x, int y, int z)
        {
            string owner = SeatManager.GetOwner(lvl, x, y, z);
            return owner.CaselessEq(p.name)
                || p.Rank >= LevelPermission.Admin;
        }

        public override void Use(Player p, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                p.Message("&cUse /help seat");
                return;
            }

            string[] args = message.SplitSpaces();
            string sub = args[0].ToLower();

            switch (sub)
            {
                case "add":
                    if (args.Length > 1)
                    {
                        if (args[1].CaselessEq("click"))
                        {
                            AddClick(p);
                            return;
                        }

                        if (args[1].CaselessEq("cuboid"))
                        {
                            p.MakeSelection(2, "%fSelecting region for %eSeat Cuboid", null, DoCuboidAdd);
                            return;
                        }
                    }

                    AddSeat(p);
                    break;

                case "remove":
                    if (args.Length > 1 && args[1].CaselessEq("cuboid"))
                    {
                        p.MakeSelection(2, "%fSelecting region for %eSeat Cuboid", null, DoCuboidRemove);
                        return;
                    }

                    HandleRemove(p, args);
                    break;

                case "info":
                    ShowInfo(p);
                    break;

                case "tp":
                    TpSeat(p, args);
                    break;

                default:
                    p.Message("&cUse /help seat");
                    break;
            }
        }

        static void AddSeat(Player p)
        {
            if (!SeatStateManager.CanEdit(p)) return;

            var pos = p.Pos.FeetBlockCoords;

            SeatManager.Add(p.Level, pos.X, pos.Y, pos.Z, p.name);

            p.Message("&aSeat set at your position.");
        }

        static void AddClick(Player p)
        {
            if (!SeatStateManager.CanEdit(p)) return;

            AddClickMode.Add(p.name);
            p.Message("&eClick a block to create a seat.");
        }

        static void HandleRemove(Player p, string[] args)
        {
            if (args.Length == 2 && args[1].CaselessEq("click"))
            {
                RemoveClickMode.Add(p.name);
                p.Message("&eClick a seat to remove it.");
                return;
            }

            int x, y, z;
            Level lvl;

            if (args.Length == 1)
            {
                var pos = p.Pos.FeetBlockCoords;
                x = pos.X; y = pos.Y; z = pos.Z;
                lvl = p.Level;
            }
            else if (args.Length == 5)
            {
                string levelName = args[1];

                if (!int.TryParse(args[2], out x) ||
                    !int.TryParse(args[3], out y) ||
                    !int.TryParse(args[4], out z))
                {
                    p.Message("&cInvalid coords.");
                    return;
                }

                lvl = LevelInfo.FindExact(levelName);
                if (lvl == null)
                {
                    p.Message("&cLevel not found.");
                    return;
                }
            }
            else
            {
                p.Message("&cUse /seat remove click OR coords");
                return;
            }

            if (!SeatManager.IsSeat(lvl, x, y, z))
            {
                p.Message("&cNo seat there.");
                return;
            }

            if (!CanModifySeat(p, lvl, x, y, z))
            {
                p.Message("&cNot your seat.");
                return;
            }

            SeatManager.Remove(lvl, x, y, z);
            p.Message("&aSeat removed.");
        }

        static void ShowInfo(Player p)
        {
            var pos = p.Pos.FeetBlockCoords;
            int x = pos.X;
            int y = pos.Y;
            int z = pos.Z;

            Level lvl = p.Level;

            if (!SeatManager.IsSeat(lvl, x, y, z))
            {
                p.Message("&cNo seat here.");
                return;
            }

            string owner = "Unknown";

            foreach (var entry in SeatManager.GetAll())
            {
                var parts = entry.Split(':');
                if (parts.Length < 4) continue;

                if (!parts[0].CaselessEq(lvl.name)) continue;
                if (!int.TryParse(parts[1], out int sx)) continue;
                if (!int.TryParse(parts[2], out int sy)) continue;
                if (!int.TryParse(parts[3], out int sz)) continue;

                if (sx != x || sy != y || sz != z) continue;

                owner = parts.Length >= 5 ? parts[4] : "Unknown";
                break;
            }

            p.Message("&e--- Seat Info ---");
            p.Message($"&fLevel: &a{lvl.name}");
            p.Message($"&fX: &a{x} &fY: &a{y} &fZ: &a{z}");
            p.Message($"&fCreated by: &e{owner}");
        }

        static void TpSeat(Player p, string[] args)
        {
            if (args.Length < 2)
            {
                p.Message("&cUsage: /seat tp <index>");
                return;
            }

            if (!int.TryParse(args[1], out int index))
            {
                p.Message("&cInvalid number.");
                return;
            }

            var seats = SeatManager.GetAll();

            int i = 0;
            foreach (var entry in seats)
            {
                if (i < index)
                {
                    i++;
                    continue;
                }

                var parts = entry.Split(':');
                if (parts.Length < 4)
                {
                    p.Message("&cCorrupted seat data.");
                    return;
                }

                string levelName = parts[0];
                if (!int.TryParse(parts[1], out int x)) continue;
                if (!int.TryParse(parts[2], out int y)) continue;
                if (!int.TryParse(parts[3], out int z)) continue;

                Level lvl = LevelInfo.FindExact(levelName);
                if (lvl == null)
                {
                    lvl = Level.Load(levelName, LevelInfo.MapPath(levelName));
                }

                if (lvl == null)
                {
                    p.Message("&cFailed to load level.");
                    return;
                }

                PlayerActions.ChangeMap(p, lvl);

                Server.MainScheduler.QueueRepeat(task =>
                {
                    if (p.level != lvl) return;

                    int px = x * 32 + 16;
                    int py = y * 32 + 91;
                    int pz = z * 32 + 16;

                    p.Pos = new Position(px, py, pz);
                    p.SendPosition(p.Pos, p.Rot);

                    Server.MainScheduler.Cancel(task);
                }, null, TimeSpan.FromMilliseconds(100));

                p.Message("&aTeleported to seat #" + index);
                return;
            }

            p.Message("&cSeat index not found.");
        }

        public static void HandleClick(Player p, ushort x, ushort y, ushort z)
        {
            SeatManager.Add(p.level, x, y + 1, z, p.name);
            p.Message("&aSeat created!");
        }

        public static void CreateCuboidSeat(Level lvl, Position a, Position b, string owner)
        {
            int minX = Math.Min(a.X, b.X);
            int minY = Math.Min(a.Y, b.Y);
            int minZ = Math.Min(a.Z, b.Z);

            int maxX = Math.Max(a.X, b.X);
            int maxY = Math.Max(a.Y, b.Y);
            int maxZ = Math.Max(a.Z, b.Z);

            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
            {
                SeatManager.Add(lvl, x, y, z, owner);
            }
        }

        public static void RemoveCuboidSeats(Level lvl, Position a, Position b, Player p)
        {
            int minX = Math.Min(a.X, b.X);
            int maxX = Math.Max(a.X, b.X);
            int minY = Math.Min(a.Y, b.Y);
            int maxY = Math.Max(a.Y, b.Y);
            int minZ = Math.Min(a.Z, b.Z);
            int maxZ = Math.Max(a.Z, b.Z);

            bool canBypass = p.Rank >= LevelPermission.Admin;

            foreach (var entry in new List<string>(SeatManager.GetAll()))
            {
                var parts = entry.Split(':');
                if (parts.Length < 4) continue;

                if (!parts[0].CaselessEq(lvl.name)) continue;

                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                int z = int.Parse(parts[3]);

                if (x < minX || x > maxX ||
                    y < minY || y > maxY ||
                    z < minZ || z > maxZ)
                    continue;

                string owner = parts.Length >= 5 ? parts[4] : "";

                if (!canBypass && !owner.CaselessEq(p.name))
                    continue;

                SeatManager.Remove(lvl, x, y, z);
            }
        }

        static bool DoCuboidAdd(Player p, Vec3S32[] marks, object state, BlockID block)
        {
            p.Message("Place or break two blocks to determine the edges.");
            var a = marks[0];
            var b = marks[1];

            CreateCuboidSeat(p.level,
                new Position(a.X, a.Y, a.Z),
                new Position(b.X, b.Y, b.Z),
                p.name);

            p.Message("&aCuboid seat created!");
            return false;
        }

        static bool DoCuboidRemove(Player p, Vec3S32[] marks, object state, BlockID block)
        {
            p.Message("Place or break two blocks to determine the edges.");
            var a = marks[0];
            var b = marks[1];

            RemoveCuboidSeats(p.level,
                new Position(a.X, a.Y, a.Z),
                new Position(b.X, b.Y, b.Z),
                p);

            p.Message("&aCuboid seats removed!");
            return false;
        }

        public override void Help(Player p)
        {
            p.Message("&e--- Seat Commands ---");

            p.Message("&aCreation:");
            p.Message("&f/seat add &7- place seat at your position");
            p.Message("&f/seat add click &7- click block to create seat");
            p.Message("&f/seat add cuboid &7- create seats in selected area");

            p.Message("&cRemoval:");
            p.Message("&f/seat remove &7- remove seat at your position");
            p.Message("&f/seat remove click &7- click seat to remove");
            p.Message("&f/seat remove cuboid &7- remove seats in selected area");

            p.Message("&bInformation:");
            p.Message("&f/seat info &7- show seat info");
            p.Message("&f/seat tp <index> &7- teleport to seat");
        }
    }
}