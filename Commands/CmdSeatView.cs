using MCGalaxy;

namespace Seatify
{
    public class CmdSeatView : Command
    {
        public override string name => "SeatView";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.AdvBuilder;

        public override void Use(Player p, string message)
        {
            if (SeatStateManager.Active.Contains(p.name))
            {
                SeatStateManager.Active.Remove(p.name);

                foreach (var entry in SeatManager.GetAll())
                {
                    var parts = entry.Split(':');
                    if (parts.Length != 4) continue;

                    string lvl = parts[0];
                    if (!lvl.CaselessEq(p.level.name)) continue;

                    int x = int.Parse(parts[1]);
                    int y = int.Parse(parts[2]);
                    int z = int.Parse(parts[3]);

                    ushort old = p.level.GetBlock((ushort)x, (ushort)y, (ushort)z);
                    p.SendBlockchange((ushort)x, (ushort)y, (ushort)z, old);
                }

                p.Message("&7Seat view disabled.");
                return;
            }

            SeatStateManager.Active.Add(p.name);

            foreach (var entry in SeatManager.GetAll())
            {
                var parts = entry.Split(':');
                if (parts.Length != 4) continue;

                string lvl = parts[0];
                if (!lvl.CaselessEq(p.level.name)) continue;

                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                int z = int.Parse(parts[3]);

                p.SendBlockchange((ushort)x, (ushort)y, (ushort)z, Block.Glass);
            }

            p.Message("&aSeat view enabled.");
        }

        public override void Help(Player p)
        {
            p.Message("&T/seatview");
            p.Message("&HToggle seat preview overlay.");
        }
    }
}
