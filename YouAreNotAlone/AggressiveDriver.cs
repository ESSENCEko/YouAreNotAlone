using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class AggressiveDriver : Nitroable
    {
        private string name;

        public AggressiveDriver(string name) : base(ListManager.EventType.AggressiveDriver) { this.name = name; }

        public bool IsCreatedIn(float radius)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            if (!Util.ThereIs(spawnedPed))
            {
                spawnedVehicle.Delete();
                return false;
            }

            Script.Wait(50);
            Function.Call(Hash.SET_DRIVER_ABILITY, spawnedPed, 1.0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedPed, 1.0f);
            Util.Tune(spawnedVehicle, true, true);

            spawnedPed.RelationshipGroup = relationship;
            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;
            spawnedPed.Task.CruiseWithVehicle(spawnedVehicle, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);

            if (!Util.BlipIsOn(spawnedPed))
            {
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Green, "Aggressive " + spawnedVehicle.FriendlyName);
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

            if (relationship != 0) Util.CleanUpRelationship(relationship, ListManager.EventType.AggressiveDriver);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed) || !Util.ThereIs(spawnedVehicle) || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped) spawnedVehicle.PlaceOnGround();
            if (Util.ThereIs(spawnedPed))
            {
                CheckDispatch();
                CheckBlockable();
            }

            return false;
        }
    }
}