using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Racer : Nitroable
    {
        private string name;
        private Vector3 goal;

        public Racer(string name, Vector3 goal) : base(EventManager.EventType.Racer)
        {
            this.name = name;
            this.goal = goal;
        }

        public bool IsCreatedIn(float radius, Road road)
        {
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

            Logger.Write("Racer: Created vehicle and driver successfully.", name);
            Script.Wait(50);
            Function.Call(Hash.SET_DRIVER_ABILITY, spawnedPed, 1.0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedPed, 1.0f);
            Util.Tune(spawnedVehicle, true, true);
            Logger.Write("Racer: Tuned racer vehicle.", name);

            spawnedPed.RelationshipGroup = relationship;
            spawnedPed.IsPriorityTargetForEnemies = true;
            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;
            Logger.Write("Racer: Characteristics are set.", name);

            if (!spawnedVehicle.Model.IsCar && !spawnedPed.IsWearingHelmet) spawnedPed.GiveHelmet(false, HelmetType.RegularMotorcycleHelmet, 4096);
            if (!Util.BlipIsOn(spawnedPed))
            {
                if (!Main.NoBlipOnCriminal)
                {
                    if (spawnedVehicle.Model.IsCar) Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.PersonalVehicleCar, (BlipColor)17, "Racer " + spawnedVehicle.FriendlyName);
                    else Util.AddBlipOn(spawnedPed, 1.0f, BlipSprite.PersonalVehicleBike, (BlipColor)17, "Racer " + spawnedVehicle.FriendlyName);
                }

                TaskSequence ts = new TaskSequence();
                ts.AddTask.DriveTo(spawnedVehicle, goal, 10.0f, 100.0f, 262692); // 4 + 32 + 512 + 262144
                ts.AddTask.Wait(10000);
                ts.AddTask.CruiseWithVehicle(spawnedVehicle, 100.0f, 262692); // 4 + 32 + 512 + 262144
                ts.Close();

                spawnedPed.Task.PerformSequence(ts);
                ts.Dispose();

                Logger.Write("Racer: Created racer successfully.", name);

                return true;
            }
            else
            {
                Logger.Error("Racer: Blip is already on racer. Abort.", name);
                Restore(true);

                return false;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write("Racer: Restore instantly.", name);

                if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write("Racer: Restore naturally.", name);
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUp(relationship);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed) || !Util.ThereIs(spawnedVehicle) || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write("Racer: Racer need to be restored.", name);
                Restore(false);

                return true;
            }

            if (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped && !spawnedVehicle.PlaceOnGround()) spawnedVehicle.PlaceOnNextStreet();

            return false;
        }

        public bool Exists()
        {
            return Util.ThereIs(spawnedPed) && Util.ThereIs(spawnedVehicle);
        }

        public float RemainingDistance { get { return spawnedPed.Position.DistanceTo(goal); } }
        public Ped Driver { get { return spawnedPed; } }
    }
}