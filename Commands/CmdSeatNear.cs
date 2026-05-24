using System;
using MCGalaxy;

namespace Seatify
{
    public class CmdSeatNear : Command
    {
        public override string name => "SeatNear";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.AdvBuilder;

        public override void Use(Player p, string message)
        {
            int radius = 10;

            if (!string.IsNullOrEmpty(message))
            {
                if (!int.TryParse(message, out radius) || radius < 1)
                {
                    p.Message("&cInvalid radius.");
                    return;
                }
            }

            var pos = p.Pos.FeetBlockCoords;

            int found = 0;

            p.Message($"&e--- Seats within {radius} blocks ---");

            foreach (var entry in SeatManager.GetAll())
            {
                var parts = entry.Split(':');

                if (parts.Length < 4) continue;

                string level = parts[0];

                if (!level.CaselessEq(p.level.name))
                    continue;

                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                int z = int.Parse(parts[3]);

                string owner = parts.Length >= 5 ? parts[4] : "Unknown";

                double dist = Math.Sqrt(
                    (x - pos.X) * (x - pos.X) +
                    (y - pos.Y) * (y - pos.Y) +
                    (z - pos.Z) * (z - pos.Z)
                );

                if (dist > radius)
                    continue;

                p.Message(
                    $"&e[{found}] &fX:&c{x} &fY:&a{y} &fZ:&9{z} " +
                    $"&7({Math.Round(dist, 1)} blocks) " +
                    $"&fOwner: &e{owner}"
                );

                found++;
            }

            if (found == 0)
            {
                p.Message("&cNo nearby seats found.");
                return;
            }

            p.Message($"&aFound {found} nearby seat(s).");
        }

        public override void Help(Player p)
        {
            p.Message("&T/seatnear <radius>");
            p.Message("&HList seats within range of your position.");
        }
    }
}