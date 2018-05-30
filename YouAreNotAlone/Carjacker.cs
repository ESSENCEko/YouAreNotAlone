using GTA;

namespace YouAreNotAlone
{
    public class Carjacker : Criminal
    {
        private float radius;
        private int trycount;

        public Carjacker() : base(EventManager.EventType.Carjacker)
        {
            this.radius = 0.0f;
            this.trycount = 0;
        }

        public bool IsCreatedIn(float radius)
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character.Position, radius);

            if (nearbyPeds.Length < 1) return false;

            spawnedPed = nearbyPeds[Util.GetRandomIntBelow(nearbyPeds.Length)];

            if (!Util.ThereIs(spawnedPed) || spawnedPed.IsPersistent || spawnedPed.Equals(Game.Player.Character) || !spawnedPed.IsHuman || spawnedPed.IsDead) return false;

            this.radius = radius;
            spawnedPed.IsPersistent = true;
            spawnedPed.IsPriorityTargetForEnemies = true;

            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;

            if (!Util.BlipIsOn(spawnedPed))
            {
                if (!Main.NoBlipOnCriminal) Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Masks, BlipColor.White, "Carjacker");

                FindNewVehicle();
                return true;
            }
            else
            {
                Restore(true);
                return false;
            }
        }

        private void FindNewVehicle()
        {
            trycount++;
            Util.NaturallyRemove(spawnedVehicle);
            spawnedVehicle = null;
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(spawnedPed.Position, radius / 2);

            if (nearbyVehicles.Length < 1) return;

            for (int cnt = 0; cnt < 5; cnt++)
            {
                Vehicle v = nearbyVehicles[Util.GetRandomIntBelow(nearbyVehicles.Length)];

                if (Util.ThereIs(v) && Util.WeCanEnter(v) && !Game.Player.Character.IsInVehicle(v) && !spawnedPed.IsInVehicle(v))
                {
                    spawnedVehicle = v;
                    break;
                }
            }

            if (!Util.ThereIs(spawnedVehicle) || !Util.WeCanGiveTaskTo(spawnedPed)) return;

            spawnedVehicle.IsPersistent = true;

            TaskSequence ts = new TaskSequence();
            ts.AddTask.ClearAll();
            ts.AddTask.EnterVehicle(spawnedVehicle, VehicleSeat.Driver, -1, 2.0f, 1);
            ts.AddTask.CruiseWithVehicle(spawnedVehicle, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
            ts.Close();

            spawnedPed.Task.PerformSequence(ts);
            ts.Dispose();
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
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUp(relationship);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed) || trycount > 5 || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (!Util.ThereIs(spawnedVehicle) || !spawnedVehicle.IsInRangeOf(spawnedPed.Position, 100.0f) || !Util.WeCanEnter(spawnedVehicle)) FindNewVehicle();
            if (Util.ThereIs(spawnedVehicle) && spawnedPed.IsInVehicle(spawnedVehicle) && spawnedPed.RelationshipGroup != relationship) spawnedPed.RelationshipGroup = relationship;

            CheckDispatch();

            return false;
        }
    }
}