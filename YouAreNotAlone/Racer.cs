using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Racer : Nitroable
    {
        private string name;
        private Vector3 goal;

        public Racer(string name, Vector3 goal) : base(ListManager.EventType.Racer)
        {
            this.name = name;
            this.goal = goal;
        }

        public bool IsCreatedIn(float radius, Road road)
        {
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

            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;
            spawnedPed.RelationshipGroup = relationship;

            if (!Util.BlipIsOn(spawnedPed))
            {
                if (spawnedVehicle.Model.IsCar) Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.PersonalVehicleCar, (BlipColor)17, "Racer " + spawnedVehicle.FriendlyName);
                else
                {
                    if (!spawnedPed.IsWearingHelmet) spawnedPed.GiveHelmet(false, HelmetType.RegularMotorcycleHelmet, 4096);

                    Util.AddBlipOn(spawnedPed, 1.0f, BlipSprite.PersonalVehicleBike, (BlipColor)17, "Racer " + spawnedVehicle.FriendlyName);
                }

                TaskSequence ts = new TaskSequence();
                ts.AddTask.DriveTo(spawnedVehicle, goal, 10.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                ts.AddTask.Wait(10000);
                ts.AddTask.CruiseWithVehicle(spawnedVehicle, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                ts.Close();

                spawnedPed.Task.PerformSequence(ts);
                ts.Dispose();
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

            if (relationship != 0) Util.CleanUpRelationship(relationship, ListManager.EventType.Racer);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed) || !Util.ThereIs(spawnedVehicle) || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped) spawnedVehicle.PlaceOnGround();

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