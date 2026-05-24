using System.Collections.Generic;
using MCGalaxy;

namespace Seatify
{
    public class CmdSeatRemoveOwner : Command
    {
        public override string name => "SeatRemoveOwner";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.Admin;

        public override void Use(Player p, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                p.Message("&cUsage: /seatremoveowner <player> [confirm]");
                return;
            }

            string[] args = message.SplitSpaces();
            string target = args[0];

            bool confirm = args.Length > 1 && args[1].CaselessEq("confirm");

            if (!confirm)
            {
                p.Message($"&cThis will delete ALL seats owned by &e{target}&c.");
                p.Message($"&eUse: /seatremoveowner {target} confirm");
                return;
            }

            List<string> toRemove = new List<string>();

            foreach (var entry in SeatManager.GetAll())
            {
                var parts = entry.Split(':');

                if (parts.Length < 5) continue;

                string owner = parts[4];

                if (owner.CaselessEq(target))
                    toRemove.Add(entry);
            }

            foreach (var entry in toRemove)
            {
                var parts = entry.Split(':');

                string levelName = parts[0];
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                int z = int.Parse(parts[3]);

                Level lvl = LevelInfo.FindExact(levelName);

                if (lvl == null)
                    continue;

                SeatManager.Remove(lvl, x, y, z);
            }

            p.Message($"&aRemoved &e{toRemove.Count} &aseats owned by &f{target}&a.");
        }

        public override void Help(Player p)
        {
            p.Message("&T/seatremoveowner <player>");
            p.Message("&HDelete all seats owned by a specific player.");
        }
    }
}