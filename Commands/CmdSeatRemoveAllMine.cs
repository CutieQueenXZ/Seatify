using System.Collections.Generic;
using MCGalaxy;

namespace Seatify
{
    public class CmdSeatRemoveAllMine : Command
    {
        public override string name => "SeatRemoveAllMine";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.AdvBuilder;

        public override void Use(Player p, string message)
        {
            if (!message.CaselessEq("confirm"))
            {
                p.Message("&cThis will remove ALL seats you own.");
                p.Message("&eUse: /seatremoveallmine confirm");
                return;
            }

            List<string> remove = new List<string>();

            foreach (var entry in SeatManager.GetAll())
            {
                var parts = entry.Split(':');

                if (parts.Length < 5)
                    continue;

                string owner = parts[4];

                if (owner.CaselessEq(p.name))
                    remove.Add(entry);
            }

            foreach (var entry in remove)
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

            p.Message($"&aRemoved &e{remove.Count} &aseat(s) owned by you.");
        }

        public override void Help(Player p)
        {
            p.Message("&T/seatremoveallmine");
            p.Message("&HRemove all seats you have created.");
        }
    }
}