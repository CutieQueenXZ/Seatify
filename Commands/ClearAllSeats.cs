using MCGalaxy;

namespace Seatify
{
    public class CmdClearAllSeats : Command
    {
        public override string name => "ClearAllSeats";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.Admin;

        public override void Use(Player p, string message)
        {
            if (!message.CaselessEq("confirm"))
            {
                p.Message("&cThis will delete ALL seats.");
                p.Message("&eType: /clearallseats confirm");
                return;
            }

            ExecuteClear(p);
        }

        void ExecuteClear(Player p)
        {
            var allSeats = SeatManager.GetAll();

            SeatManager.ClearAll();
            SeatStateManager.Active.Clear();
            SeatifyPlugin.Sitting.Clear();

            foreach (var pl in PlayerInfo.Online.Items)
            {
                if (pl == null || pl.Level == null) continue;

                foreach (var e in allSeats)
                {
                    var parts = e.Split(':');
                    if (parts.Length != 4) continue;

                    string level = parts[0];
                    if (!pl.Level.name.CaselessEq(level)) continue;

                    int x = int.Parse(parts[1]);
                    int y = int.Parse(parts[2]);
                    int z = int.Parse(parts[3]);
                    string owner = parts.Length >= 5 ? parts[4] : "Unknown";

                    pl.SendBlockchange((ushort)x, (ushort)y, (ushort)z, Block.Air);
                }
            }

            p.Message("&aAll seats cleared.");
        }

        public override void Help(Player p)
        {
            p.Message("&T/clearallseats");
            p.Message("&HDanger: removes every seat from the server.");
        }
    }
}