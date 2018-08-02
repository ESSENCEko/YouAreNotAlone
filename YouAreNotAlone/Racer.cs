using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Racer : Nitroable
    {
        private string name;
        private string blipName;
        private Vector3 goal;

        public Racer(string name, Vector3 goal) : base(EventManager.EventType.Racer)
        {
            this.name = name;
            this.blipName = "";
            this.goal = goal;
            Logger.Write(true, "Racer: Creating racer.", this.name);
        }

        public bool IsCreatedIn(float radius, Road road)
        {
            if (relationship == 0 || road == null) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Error("Racer: Couldn't create vehicle. Abort.", name);

                return false;
            }

            spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            if (!Util.ThereIs(spawnedPed))
            {
                Logger.Error("Racer: Couldn't create driver. Abort.", name);
                spawnedVehicle.Delete();

                return false;
            }

            Logger.Write(false, "Racer: Created vehicle and driver successfully.", name);
            Script.Wait(50);
            Function.Call(Hash.SET_DRIVER_ABILITY, spawnedPed, 1.0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedPed, 1.0f);

            SetExhausts();
            Util.Tune(spawnedVehicle, true, true, true);
            Logger.Write(false, "Racer: Tuned racer vehicle.", name);

            spawnedPed.RelationshipGroup = relationship;
            spawnedPed.IsPriorityTargetForEnemies = true;
            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;
            Logger.Write(false, "Racer: Characteristics are set.", name);

            if (Util.BlipIsOn(spawnedPed))
            {
                Logger.Error("Racer: Blip is already on racer. Abort.", name);
                Restore(true);

                return false;
            }
            else
            {
                if (!spawnedVehicle.Model.IsCar && !spawnedPed.IsWearingHelmet) spawnedPed.GiveHelmet(false, HelmetType.RegularMotorcycleHelmet, 4096);

                TaskSequence ts = new TaskSequence();
                ts.AddTask.DriveTo(spawnedVehicle, goal, 10.0f, 100.0f, 262692); // 4 + 32 + 512 + 262144
                ts.AddTask.Wait(10000);
                ts.AddTask.CruiseWithVehicle(spawnedVehicle, 100.0f, 262692); // 4 + 32 + 512 + 262144
                ts.Close();

                spawnedPed.Task.PerformSequence(ts);
                ts.Dispose();

                Logger.Write(false, "Racer: Created racer successfully.", name);
                blipName += VehicleInfo.GetNameOf(spawnedVehicle.Model.Hash);

                return true;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, "Racer: Restore instantly.", name);

                if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(false, "Racer: Restore naturally.", name);
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUp(relationship);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed) || !Util.ThereIs(spawnedVehicle) || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "Racer: Racer need to be restored.", name);

                return true;
            }

            if (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped && !spawnedVehicle.PlaceOnGround()) spawnedVehicle.PlaceOnNextStreet();
            if (spawnedPed.IsSittingInVehicle(spawnedVehicle))
            {
                if (!Util.BlipIsOn(spawnedVehicle))
                {
                    if (spawnedVehicle.Model.IsCar) Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.PersonalVehicleCar, (BlipColor)17, "Racer " + blipName);
                    else Util.AddBlipOn(spawnedVehicle, 1.0f, BlipSprite.PersonalVehicleBike, (BlipColor)17, "Racer " + blipName);
                }
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
            }
            else
            {
                if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();
                if (!Util.BlipIsOn(spawnedPed))
                {
                    if (spawnedVehicle.Model.IsCar) Util.AddBlipOn(spawnedPed, 0.6f, BlipSprite.PersonalVehicleCar, (BlipColor)17, "Racer");
                    else Util.AddBlipOn(spawnedPed, 0.8f, BlipSprite.PersonalVehicleBike, (BlipColor)17, "Racer");
                }
            }

            return false;
        }

        public bool Exists() => Util.ThereIs(spawnedPed) && Util.ThereIs(spawnedVehicle);

        public float RemainingDistance => spawnedPed.Position.DistanceTo(goal);

        public Ped Driver => spawnedPed;
    }
}