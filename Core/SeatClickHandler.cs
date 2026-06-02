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

        static void OnLeave(Player p, string reason)
        {
            Awaiting.Remove(p.name);
        }
    }
}