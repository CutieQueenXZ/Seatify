using System;
using MCGalaxy;

namespace Seatify
{
    public class CmdListSeats : Command
    {
        public override string name => "ListSeats";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.AdvBuilder;

        const int PAGE_SIZE = 10;

        public override void Use(Player p, string message)
        {
            string[] args = message.SplitSpaces();

            string filterLevel = "all";
            int page = 1;

            if (args.Length >= 1 && args[0] != "")
                filterLevel = args[0];

            if (args.Length >= 2)
                int.TryParse(args[1], out page);

            if (page <= 0) page = 1;

            var seats = SeatManager.GetAll();

            // filter first
            var filtered = new System.Collections.Generic.List<string>();

            foreach (var entry in seats)
            {
                var parts = entry.Split(':');
                if (parts.Length != 4) continue;

                string level = parts[0];

                if (filterLevel != "all" &&
                    !level.CaselessEq(filterLevel))
                    continue;

                filtered.Add(entry);
            }

            int total = filtered.Count;
            int start = (page - 1) * PAGE_SIZE;

            p.Message("&e--- Seatify Seats ---");
            p.Message($"&7Filter: &f{filterLevel} &7Page: &f{page}");

            if (start >= total)
            {
                p.Message("&cNo seats on this page.");
                return;
            }

            for (int i = start; i < Math.Min(start + PAGE_SIZE, total); i++)
            {
                var parts = filtered[i].Split(':');

                string level = parts[0];
                string x = parts[1];
                string y = parts[2];
                string z = parts[3];
                string owner = parts.Length >= 5 ? parts[4] : "Unknown";

                p.Message($"&e[{i}] &f{level} &7X:{x} Y:{y} Z:{z}");
            }

            p.Message($"&aShowing {start + 1}-{Math.Min(start + PAGE_SIZE, total)} of {total}");
        }

        public override void Help(Player p)
        {
            p.Message("&T/listseats [level|all] [page]");
            p.Message("&HBrowse all seats on a level or server-wide.");
        }
    }
}