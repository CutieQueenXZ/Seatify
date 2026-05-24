using System.Collections.Generic;
using System.ComponentModel;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using Seatify.Core;

namespace Seatify
{
    public static class SeatClickHandler
    {
        public static HashSet<string> Awaiting = new HashSet<string>();

        public static class SeatInfoState
        {
            public static HashSet<string> ClickMode = new HashSet<string>();
        }

        public static void Init()
        {
            OnPlayerClickEvent.Register(OnBlockChanged, Priority.Normal);
            OnPlayerDisconnectEvent.Register(OnLeave, Priority.Normal);
        }

        static void OnBlockChanged(Player p, MouseButton button, MouseAction action,
            ushort yaw, ushort pitch, byte entity,
            ushort x, ushort y, ushort z, TargetBlockFace face)
        {

            if (CmdSeat.AddClickMode.Contains(p.name))
            {
                CmdSeat.AddClickMode.Remove(p.name);
                CmdSeat.HandleClick(p, x, y, z);
                return;
            }
            
            if (CmdSeat.CuboidMode.Contains(p.name))
            {
                HandleCuboidClick(p, x, y, z);
                return;
            }

            if (CmdSeat.RemoveCuboidMode.Contains(p.name))
            {
                HandleRemoveCuboidClick(p, x, y, z);
                return;
            }

            if (CmdSeat.RemoveClickMode.Contains(p.name))
            {
                CmdSeat.RemoveClickMode.Remove(p.name);

                int rx = x;
                int ry = y + 1;
                int rz = z;

                if (!SeatManager.IsSeat(p.level, rx, ry, rz))
                {
                    p.Message("&cNo seat at that position.");
                    return;
                }

                string owner = SeatManager.GetOwner(p.level, rx, ry, rz);

                bool isOwner = owner.CaselessEq(p.name);
                bool isAdmin = p.Rank >= LevelPermission.Admin;

                if (!isOwner && !isAdmin)
                {
                    p.Message("&cNot your seat.");
                    return;
                }

                SeatManager.Remove(p.level, rx, ry, rz);

                p.Message($"&aSeat removed at {rx}, {ry}, {rz}");
                return;
            }

            if (!Awaiting.Contains(p.name)) return;
            Awaiting.Remove(p.name);

            // IMPORTANT: use clicked block, BUT adjust like sitting systems expect
            int sx = x;
            int sy = y + 1; // seat is usually above the block you click
            int sz = z;

            if (!SeatStateManager.CanEdit(p)) return;

            SeatManager.Add(p.level, sx, sy, sz, p.name);

            p.Message("&aSeat saved from clicked block!");
        }

        static void HandleCuboidClick(Player p, ushort x, ushort y, ushort z)
        {
            if (CmdSeat.CuboidLock.Contains(p.name))
            {
                CmdSeat.CuboidLock.Remove(p.name);
                return;
            }

            // FIRST CLICK
            if (!CmdSeat.FirstPoint.ContainsKey(p.name))
            {
                CmdSeat.FirstPoint[p.name] = new Position(x, y, z);

                CmdSeat.CuboidLock.Add(p.name);
                p.Message("&aFirst point set. Now click second corner.");
                return;
            }

            // SECOND CLICK
            var p1 = CmdSeat.FirstPoint[p.name];
            var p2 = new Position(x, y, z);

            CmdSeat.FirstPoint.Remove(p.name);
            CmdSeat.CuboidMode.Remove(p.name);

            CmdSeat.CreateCuboidSeat(p.level, p1, p2, p.name);

            p.Message("&aCuboid seat created!");
        }

        static void HandleRemoveCuboidClick(Player p, ushort x, ushort y, ushort z)
        {
            if (CmdSeat.CuboidLock.Contains(p.name))
            {
                CmdSeat.CuboidLock.Remove(p.name);
                return;
            }
            if (!CmdSeat.RemoveFirstPoint.ContainsKey(p.name))
            {
                CmdSeat.RemoveFirstPoint[p.name] = new Position(x, y, z);

                CmdSeat.CuboidLock.Add(p.name);
                p.Message("&aFirst point set. Now click second corner.");
                return;
            }

            var p1 = CmdSeat.RemoveFirstPoint[p.name];
            var p2 = new Position(x, y, z);

            CmdSeat.RemoveFirstPoint.Remove(p.name);
            CmdSeat.RemoveCuboidMode.Remove(p.name);

            CmdSeat.RemoveCuboidSeats(p.level, p1, p2, p);

            p.Message("&aCuboid seats removed!");
        }

        static void OnLeave(Player p, string reason)
        {
            Awaiting.Remove(p.name);

            CmdSeat.CuboidMode.Remove(p.name);
            CmdSeat.FirstPoint.Remove(p.name);
        }
    }
}