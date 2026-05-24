using System.Collections.Generic;
using MCGalaxy;

namespace Seatify
{
    public class CmdSeatStats : Command
    {
        public override string name => "SeatStats";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.AdvBuilder;

        public override void Use(Player p, string message)
        {
            int totalSeats = 0;
            int levelSeats = 0;

            Dictionary<string, int> owners =
                new Dictionary<string, int>();

            foreach (var entry in SeatManager.GetAll())
            {
                var parts = entry.Split(':');

                if (parts.Length < 5) continue;

                string level = parts[0];
                string owner = parts[4];

                totalSeats++;

                if (level.CaselessEq(p.level.name))
                    levelSeats++;

                if (!owners.ContainsKey(owner))
                    owners[owner] = 0;

                owners[owner]++;
            }

            string topOwner = "Nobody";
            int topCount = 0;

            foreach (var pair in owners)
            {
                if (pair.Value > topCount)
                {
                    topOwner = pair.Key;
                    topCount = pair.Value;
                }
            }

            p.Message("&e--- Seatify Stats ---");
            p.Message($"&fTotal seats: &a{totalSeats}");
            p.Message($"&fSeats in this level: &a{levelSeats}");
            p.Message($"&fSeat owners: &a{owners.Count}");
            p.Message($"&fTop owner: &e{topOwner} &7({topCount})");
        }

        public override void Help(Player p)
        {
            p.Message("&T/seatstats");
            p.Message("&HView Seatify statistics.");
        }
    }
}