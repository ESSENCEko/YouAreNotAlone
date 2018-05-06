using GTA;
using GTA.Native;

namespace YouAreNotAlone
{
    public abstract class EmergencyHeli : Emergency
    {
        public EmergencyHeli(string name, Entity target) : base(name, target) { }

        private void SetPedAsCop(Ped p)
        {
            if (Util.ThereIs(p))
            {
                p.AlwaysKeepTask = false;
                p.BlockPermanentEvents = false;
                Function.Call(Hash.SET_PED_AS_COP, p, true);
            }
        }

        public override bool ShouldBeRemoved()
        {
            int alive = 0;

            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }

                if (members[i].IsInRangeOf(target.Position, 50.0f) || !Util.ThereIs(target) || target.IsDead) SetPedAsCop(members[i]);
                if (members[i].IsDead)
                {
                    members[i].MarkAsNoLongerNeeded();
                    members.RemoveAt(i);
                }
                else if (!members[i].Equals(spawnedVehicle.Driver)) alive++;
            }

            if (!Util.ThereIs(spawnedVehicle) || alive < 1 || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p))
                    {
                        SetPedAsCop(p);
                        p.MarkAsNoLongerNeeded();
                    }
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                return true;
            }

            return false;
        }
    }
}