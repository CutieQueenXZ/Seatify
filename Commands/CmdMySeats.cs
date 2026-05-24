using MCGalaxy;

namespace Seatify
{
    public class CmdMySeats : Command
    {
        public override string name => "MySeats";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.AdvBuilder;

        public override void Use(Player p, string message)
        {
            int page = 1;

            if (!string.IsNullOrEmpty(message))
            {
                if (!int.TryParse(message, out page) || page < 1)
                {
                    p.Message("&cInvalid page number.");
                    return;
                }
            }

            var seats = new System.Collections.Generic.List<string>();

            foreach (var entry in SeatManager.GetAll())
            {
                var parts = entry.Split(':');

                if (parts.Length < 5) continue;

                string owner = parts[4];

                if (owner.CaselessEq(p.name))
                    seats.Add(entry);
            }

            if (seats.Count == 0)
            {
                p.Message("&cYou have no seats.");
                return;
            }

            int perPage = 10;
            int totalPages = (seats.Count + perPage - 1) / perPage;

            if (page > totalPages)
            {
                p.Message("&cPage does not exist.");
                return;
            }

            int start = (page - 1) * perPage;
            int end = System.Math.Min(start + perPage, seats.Count);

            p.Message($"&e--- Your Seats &7(Page {page}/{totalPages}) &e---");

            for (int i = start; i < end; i++)
            {
                var parts = seats[i].Split(':');

                string level = parts[0];
                string x = parts[1];
                string y = parts[2];
                string z = parts[3];

                p.Message(
                    $"&e[{i}] &f{level} " +
                    $"&cX:{x} &aY:{y} &9Z:{z}"
                );
            }

            p.Message($"&aTotal seats: {seats.Count}");
        }

            public override void Help(Player p)
            {
                p.Message("&T/myseats <page>");
                p.Message("&HShow seats you have created.");
            }
    }
}