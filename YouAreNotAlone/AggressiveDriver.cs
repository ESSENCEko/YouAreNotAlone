using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class AggressiveDriver : Nitroable, ICheckable
    {
        private string name;
        private string blipName;

        public AggressiveDriver(string name) : base(EventManager.EventType.AggressiveDriver)
        {
            this.name = name;
            this.blipName = "";
            Logger.Write(true, "AggressiveDriver event selected.", this.name);
        }

        public bool IsCreatedIn(float radius)
        {
            if (relationship == 0) return false;

            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero))
            {
                Logger.Error("AggressiveDriver: Couldn't find safe position. Abort.", name);

                return false;
            }

            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                if (!road.Position.Equals(Vector3.Zero))
                {
                    Logger.Write(false, "AggressiveDriver: Found proper road.", name);

                    break;
                }
            }

            if (road.Position.Equals(Vector3.Zero))
            {
                Logger.Error("AggressiveDriver: Couldn't find proper road. Abort.", name);

                return false;
            }

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Error("AggressiveDriver: Couldn't create vehicle. Abort.", name);

                return false;
            }

            spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            if (!Util.ThereIs(spawnedPed))
            {
                Logger.Error("AggressiveDriver: Couldn't create driver. Abort.", name);
                spawnedVehicle.Delete();

                return false;
            }

            Logger.Write(false, "AggressiveDriver: Created vehicle and driver successfully.", name);
            Script.Wait(50);
            Function.Call(Hash.SET_DRIVER_ABILITY, spawnedPed, 1.0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedPed, 1.0f);

            SetExhausts();
            Util.Tune(spawnedVehicle, true, true, true);
            Logger.Write(false, "AggressiveDriver: Tuned aggressive vehicle.", name);

            spawnedPed.RelationshipGroup = relationship;
            spawnedPed.IsPriorityTargetForEnemies = true;
            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;
            spawnedPed.Task.CruiseWithVehicle(spawnedVehicle, 100.0f, 262692); // 4 + 32 + 512 + 262144
            Logger.Write(false, "AggressiveDriver: Characteristics are set.", name);

            if (Util.BlipIsOn(spawnedPed))
            {
                Logger.Error("AggressiveDriver: Blip is already on aggressive driver. Abort.", name);
                Restore(true);

                return false;
            }
            else
            {
                Logger.Write(false, "AggressiveDriver: Created aggressive driver successfully.", name);
                blipName += VehicleInfo.GetNameOf(spawnedVehicle.Model.Hash);

                return true;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, "AggressiveDriver: Restore instantly.", name);

                if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(false, "AggressiveDriver: Restore naturally.", name);
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUp(relationship);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed) || !Util.ThereIs(spawnedVehicle) || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "AggressiveDriver: Aggressive driver need to be restored.", name);

                return true;
            }

            if (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped && !spawnedVehicle.PlaceOnGround()) spawnedVehicle.PlaceOnNextStreet();
            if (spawnedPed.IsSittingInVehicle(spawnedVehicle))
            {
                if (!Util.BlipIsOn(spawnedVehicle)) Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Green, "Aggressive " + blipName);
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
            }
            else
            {
                if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();
                if (!Util.BlipIsOn(spawnedPed)) Util.AddBlipOn(spawnedPed, 0.6f, BlipSprite.PersonalVehicleCar, BlipColor.Green, "Aggressive Driver");
            }

            CheckDispatch();
            CheckBlockable();

            return false;
        }

        public void CheckAbilityUsable() => CheckNitroable();
    }
}