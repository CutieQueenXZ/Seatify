using System;
using MCGalaxy;
using System.Collections.Generic;
using MCGalaxy.Tasks;

namespace Seatify
{
    public class SeatifyPlugin : Plugin
    {
        public override string name => "Seatify";
        public override string MCGalaxy_Version => "1.9.4.10";

        static Dictionary<string, HashSet<string>> LastShown = new();

        public static HashSet<string> Sitting = new HashSet<string>();

        private SchedulerTask seatTickTask;
        private SchedulerTask playerTickTask;

        public override void Load(bool startup)
        {
            Command.Register(new CmdSeat());
            Command.Register(new CmdSeatFreeze());
            Command.Register(new CmdSeatStats());
            Command.Register(new CmdSeatNear());
            Command.Register(new CmdSeatRemoveAllMine());
            Command.Register(new CmdSeatRemoveOwner());
            Command.Register(new CmdSeatView());
            Command.Register(new CmdListSeats());
            Command.Register(new CmdMySeats());
            Command.Register(new CmdClearAllSeats());
            SeatManager.Load();

            SeatStateManager.LoadFreeze();

            SeatClickHandler.Init();

            playerTickTask = Server.MainScheduler.QueueRepeat(CheckPlayers, null,
                TimeSpan.FromMilliseconds(250));

            seatTickTask = Server.MainScheduler.QueueRepeat(SeatViewTick, null,
                TimeSpan.FromMilliseconds(250));
        }

        public override void Unload(bool shutdown)
        {
            SeatManager.Save();

            LastShown.Clear();

            SeatStateManager.SaveFreeze();

            if (playerTickTask != null)
                Server.MainScheduler.Cancel(playerTickTask);

            if (seatTickTask != null)
                Server.MainScheduler.Cancel(seatTickTask);

            var setSeat = Command.Find("SetSeat");
            if (setSeat != null) Command.Unregister(setSeat);

            var listSeats = Command.Find("ListSeats");
            if (listSeats != null) Command.Unregister(listSeats);

            var unsetSeat = Command.Find("UnsetSeat");
            if (unsetSeat != null) Command.Unregister(unsetSeat);

            var TpSeat = Command.Find("TpSeat");
            if (TpSeat != null) Command.Unregister(TpSeat);

            var ClickSeat = Command.Find("CSeat");
            if (ClickSeat != null) Command.Unregister(ClickSeat);

            var SeatView = Command.Find("SeatView");
            if (SeatView != null) Command.Unregister(SeatView);

            var RemoveSeat = Command.Find("RemoveSeat");
            if (RemoveSeat != null) Command.Unregister(RemoveSeat);

            var SeatInfo = Command.Find("SeatInfo");
            if (SeatInfo != null) Command.Unregister(SeatInfo);

            var mySeats = Command.Find("MySeats");
            if (mySeats != null) Command.Unregister(mySeats);

            var stats = Command.Find("SeatStats");
            if (stats != null) Command.Unregister(stats);

            var near = Command.Find("SeatNear");
            if (near != null) Command.Unregister(near);

            var removeMine = Command.Find("SeatRemoveAllMine");
            if (removeMine != null) Command.Unregister(removeMine);

            var SeatRemoveOwner = Command.Find("SeatRemoveOwner");
            if (SeatRemoveOwner != null) Command.Unregister(SeatRemoveOwner);

            var SeatFreeze = Command.Find("SeatFreeze");
            if (SeatFreeze != null) Command.Unregister(SeatFreeze);

            var ClearAllSeats = Command.Find("ClearAllSeats");
            if (ClearAllSeats != null) Command.Unregister(ClearAllSeats);
        }

        void CheckPlayers(object state)
        {
            foreach (var p in PlayerInfo.Online.Items)
            {
                if (p == null || p.Level == null) continue;

                var pos = p.Pos.FeetBlockCoords;
                if (!p.Level.IsValidPos(pos.X, pos.Y, pos.Z)) continue;

                bool isSeat = SeatManager.IsSeat(p.Level, pos.X, pos.Y, pos.Z);

                if (isSeat)
                {
                    Sitting.Add(p.name);
                }
                else
                {
                    Sitting.Remove(p.name);
                }

                string model = Sitting.Contains(p.name) ? "sit" : "humanoid";

                if (p.Model != model)
                    p.UpdateModel(model);
            }
        }

        static HashSet<string> GetOrCreate(string name)
        {
            if (!LastShown.TryGetValue(name, out var set))
                LastShown[name] = set = new HashSet<string>();
            return set;
        }

        void SeatViewTick(object state)
        {
            foreach (var p in PlayerInfo.Online.Items)
            {
                if (p == null || p.Level == null) continue;
                if (!SeatViewManager.Active.Contains(p.name)) continue;

                var shown = GetOrCreate(p.name);

                foreach (var entry in SeatManager.GetAll())
                {
                    var parts = entry.Split(':');
                    if (parts.Length != 4) continue;

                    string level = parts[0];
                    if (!p.Level.name.CaselessEq(level)) continue;

                    string key = entry;
                    if (shown.Contains(key)) continue;

                    int x = int.Parse(parts[1]);
                    int y = int.Parse(parts[2]);
                    int z = int.Parse(parts[3]);
                    string owner = parts.Length >= 5 ? parts[4] : "Unknown";

                    p.SendBlockchange((ushort)x, (ushort)y, (ushort)z, Block.Glass);

                    shown.Add(key);
                }
            }
        }
    }
}