using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Terrorist : Criminal
    {
        private string name;

        public Terrorist(string name) : base(ListManager.EventType.Terrorist) { this.name = name; }

        public bool IsCreatedIn(float radius)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            spawnedPed = spawnedVehicle.CreatePedOnSeat(VehicleSeat.Driver, "g_m_m_chicold_01");

            if (!Util.ThereIs(spawnedPed))
            {
                spawnedVehicle.Delete();
                return false;
            }

            Script.Wait(50);
            Util.Tune(spawnedVehicle, false, false);

            spawnedPed.RelationshipGroup = relationship;
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 46, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 5, true);

            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;
            spawnedPed.Task.FightAgainstHatedTargets(400.0f);

            if (!Util.BlipIsOn(spawnedPed))
            {
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Tank, BlipColor.Red, "Terrorist " + spawnedVehicle.FriendlyName);
                return true;
            }
            else
            {
                Restore(true);
                return false;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                if (Util.ThereIs(spawnedPed))
                {
                    spawnedPed.MarkAsNoLongerNeeded();

                    if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                }
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();
            }

            if (relationship != 0) Util.CleanUpRelationship(relationship, ListManager.EventType.Terrorist);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed) || !Util.ThereIs(spawnedVehicle) || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (Util.ThereIs(spawnedPed))
            {
                CheckDispatch();
                CheckBlockable();
            }

            return false;
        }

        private new void CheckDispatch()
        {
            if (dispatchCooldown < 15) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.AnyEmergencyIsNear(spawnedPed.Position, ListManager.EventType.Army)) YouAreNotAlone.DispatchAgainst(spawnedPed, type);
            }
        }
    }
}