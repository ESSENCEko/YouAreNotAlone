using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Terrorist : Criminal
    {
        private string name;

        public Terrorist(string name) : base(YouAreNotAlone.CrimeType.Terrorist)
        {
            this.name = name;
        }

        public bool IsCreatedIn(float radius)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, Util.GetRandomInt(360), true);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            spawnedPed = spawnedVehicle.CreatePedOnSeat(VehicleSeat.Driver, "g_m_m_chicold_01");

            if (!Util.ThereIs(spawnedPed))
            {
                spawnedVehicle.Delete();
                return false;
            }

            relationship = Util.NewRelationship(YouAreNotAlone.CrimeType.Terrorist);

            if (relationship == 0)
            {
                Restore();
                return false;
            }

            Script.Wait(50);
            Util.Tune(spawnedVehicle, false, false);

            spawnedPed.RelationshipGroup = relationship;
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 46, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 5, true);

            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;

            spawnedPed.ShootRate = 100;
            spawnedPed.Task.FightAgainstHatedTargets(400.0f);

            if (!Util.BlipIsOn(spawnedPed))
            {
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Tank, BlipColor.Red, "Terrorist " + spawnedVehicle.FriendlyName);
                return true;
            }
            else
            {
                Restore();
                return false;
            }
        }

        public override void Restore()
        {
            if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
            if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            if (relationship != 0) Util.CleanUpRelationship(spawnedPed.RelationshipGroup);
        }

        private new void CheckDispatch()
        {
            if (dispatchCooldown < 15) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.AnyEmergencyIsNear(spawnedPed.Position, YouAreNotAlone.EmergencyType.Army)) YouAreNotAlone.DispatchAgainst(spawnedPed, type);
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed))
            {
                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();
                if (relationship != 0) Util.CleanUpRelationship(relationship);

                return true;
            }

            if (!Util.ThereIs(spawnedVehicle))
            {
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                if (relationship != 0) Util.CleanUpRelationship(relationship);

                return true;
            }

            if (spawnedPed.IsDead || !spawnedVehicle.IsDriveable || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                if (spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();
                if (relationship != 0) Util.CleanUpRelationship(relationship);

                return true;
            }

            if (Util.ThereIs(spawnedPed)) CheckDispatch();

            return false;
        }
    }
}