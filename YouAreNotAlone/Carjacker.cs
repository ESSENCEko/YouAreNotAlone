using GTA;

namespace YouAreNotAlone
{
    public class Carjacker : Criminal
    {
        private float radius;
        private int lastVehicle;
        private int trycount;

        public Carjacker() : base(EventManager.EventType.Carjacker)
        {
            this.radius = 0.0f;
            this.lastVehicle = 0;
            this.trycount = 0;
            Logger.Write(true, "Carjacker event selected.", "");
        }

        public bool IsCreatedIn(float radius)
        {
            if (relationship == 0) return false;

            Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character.Position, radius);

            if (nearbyPeds.Length < 1)
            {
                Logger.Error("Carjacker: Couldn't find peds nearby. Abort", "");

                return false;
            }

            this.radius = radius;

            for (int i = 0; i < 5; i++)
            {
                Ped selectedPed = nearbyPeds[Util.GetRandomIntBelow(nearbyPeds.Length)];

                if (!Util.ThereIs(selectedPed) || Util.BlipIsOn(selectedPed) || selectedPed.IsPersistent || selectedPed.Equals(Game.Player.Character) || !selectedPed.IsHuman || selectedPed.IsDead)
                {
                    Logger.Write(false, "Carjacker: Couldn't use selected ped.", "");

                    continue;
                }

                Logger.Write(false, "Carjacker: Found proper ped.", "");
                spawnedPed = selectedPed;
                spawnedPed.IsPersistent = true;
                spawnedPed.IsPriorityTargetForEnemies = true;

                spawnedPed.AlwaysKeepTask = true;
                spawnedPed.BlockPermanentEvents = true;
                Logger.Write(false, "Carjacker: Characteristics are set.", "");

                if (spawnedPed.IsInVehicle()) lastVehicle = spawnedPed.CurrentVehicle.Handle;

                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Masks, BlipColor.White, "Carjacker");
                Logger.Write(false, "Carjacker: Selected carjacker successfully.", "");
                FindNewVehicle();

                return true;
            }

            Logger.Error("Carjacker: Couldn't select carjacker. Abort.", "");
            Restore(true);

            return false;
        }

        private void FindNewVehicle()
        {
            Logger.Write(false, "Carjacker: Finding new vehicle to jack.", "");
            trycount++;

            Util.NaturallyRemove(spawnedVehicle);
            spawnedVehicle = null;
            Logger.Write(false, "Carjacker: Unset previous vehicle.", "");

            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(spawnedPed.Position, radius / 2);

            if (nearbyVehicles.Length < 1)
            {
                Logger.Write(false, "Carjacker: Couldn't find vehicles nearby. Abort finding.", "");

                return;
            }

            for (int cnt = 0; cnt < 5; cnt++)
            {
                Vehicle v = nearbyVehicles[Util.GetRandomIntBelow(nearbyVehicles.Length)];

                if (Util.ThereIs(v) && Util.WeCanEnter(v) && !spawnedPed.IsInVehicle(v) && v.Handle != lastVehicle && (Main.CriminalsCanFightWithPlayer || !Game.Player.Character.IsInVehicle(v)))
                {
                    Logger.Write(false, "Carjacker: Found proper vehicle.", "");
                    spawnedVehicle = v;

                    break;
                }
            }

            if (!Util.ThereIs(spawnedVehicle) || !Util.WeCanGiveTaskTo(spawnedPed))
            {
                Logger.Error("Carjacker: Couldn't find proper vehicle. Abort finding.", "");

                return;
            }

            spawnedVehicle.IsPersistent = true;
            TaskSequence ts = new TaskSequence();
            ts.AddTask.EnterVehicle(spawnedVehicle, VehicleSeat.Driver, -1, 2.0f, 1);
            ts.AddTask.CruiseWithVehicle(spawnedVehicle, 100.0f, 262692); // 4 + 32 + 512 + 262144
            ts.Close();

            spawnedPed.Task.PerformSequence(ts);
            ts.Dispose();
            Logger.Write(false, "Carjacker: Jack new vehicle.", "");
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, "Carjacker: Restore instantly.", "");

                if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(false, "Carjacker: Restore naturally.", "");
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUp(relationship);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed) || trycount > 5 || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "Carjacker: Carjacker need to be restored.", "");

                return true;
            }

            if (!Util.ThereIs(spawnedVehicle) || !spawnedVehicle.IsInRangeOf(spawnedPed.Position, 50.0f) || !Util.WeCanEnter(spawnedVehicle) || spawnedPed.IsBeingJacked) FindNewVehicle();
            if (Util.ThereIs(spawnedVehicle) && spawnedPed.IsInVehicle(spawnedVehicle) && spawnedPed.RelationshipGroup != relationship) spawnedPed.RelationshipGroup = relationship;

            CheckDispatch();

            return false;
        }
    }
}