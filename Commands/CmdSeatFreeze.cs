using MCGalaxy;
using Seatify.Core;
using System.Collections.Generic;

namespace Seatify
{
    public class CmdSeatFreeze : Command
    {
        public override string name => "SeatFreeze";
        public override string type => "Seatify";
        public override LevelPermission defaultRank => LevelPermission.Admin;

        public override void Use(Player p, string message)
        {
            string[] args = message.SplitSpaces();

            // /seatfreeze list
            if (args.Length >= 1 && args[0].CaselessEq("list"))
            {
                if (SeatStateManager.FrozenLevels.Count == 0)
                {
                    p.Message("&7No frozen levels.");
                    return;
                }

                p.Message("&e--- Frozen Seat Levels ---");

                foreach (string lvl in SeatStateManager.FrozenLevels)
                {
                    string by = SeatStateManager.GetFrozenBy(lvl);
                    string freezeReason = SeatStateManager.GetFrozenReason(lvl);

                    p.Message($"&c{lvl} &7| by &e{by} &7| &f{freezeReason}");
                }

                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                p.Message("&cUsage:");
                p.Message("&e/seatfreeze <level> [reason...]");
                p.Message("&e/seatfreeze list");
                return;
            }

            string level = args[0];

            string reason = SeatStateManager.GetFrozenReason(level);
            if (args.Length > 1)
                reason = string.Join(" ", args, 1, args.Length - 1);

            if (SeatStateManager.IsFrozen(level))
            {
                SeatStateManager.Unfreeze(level);
                p.Message($"&aUnfrozen level: &e{level}");
            }
            else
            {
                SeatStateManager.Freeze(level, p.name, reason);
                p.Message($"&cFrozen level: &e{level}");
                p.Message($"&7Reason: {reason}");
            }
        }

        public override void Help(Player p)
        {
            p.Message("&T/seatfreeze <level>");
            p.Message("&HToggle seat editing protection for a level.");
        }
    }
}