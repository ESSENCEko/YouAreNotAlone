using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public abstract class Emergency : EntitySet
    {
        protected List<Ped> members;
        protected string name;
        protected Entity target;

        public Emergency(string name, Entity target) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
            this.target = target;
        }

        public abstract bool IsCreatedIn(Vector3 safePosition, List<string> models);

        public override void Restore()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p)) p.Delete();
            }

            if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();

            members.Clear();
        }

        private void SetPedAsCop(Ped p)
        {
            if (Util.ThereIs(p))
            {
                p.AlwaysKeepTask = false;
                p.BlockPermanentEvents = false;
                Function.Call(Hash.SET_PED_AS_COP, p, true);
                p.MarkAsNoLongerNeeded();
            }
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }

                if (members[i].IsInRangeOf(target.Position, 50.0f)) SetPedAsCop(members[i]);
                if (members[i].IsDead)
                {
                    members[i].MarkAsNoLongerNeeded();
                    members.RemoveAt(i);
                }
            }

            if (!Util.ThereIs(spawnedVehicle) || !Util.ThereIs(target) || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) SetPedAsCop(p);
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                return true;
            }

            return false;
        }
    }
}