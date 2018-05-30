using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Terrorist : Criminal
    {
        private string name;

        public Terrorist(string name) : base(EventManager.EventType.Terrorist) { this.name = name; }

        public bool IsCreatedIn(float radius)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                if (!road.Position.Equals(Vector3.Zero)) break;
            }

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

            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, spawnedPed, 0, false);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 17, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 46, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 5, true);

            spawnedPed.RelationshipGroup = relationship;
            spawnedPed.IsPriorityTargetForEnemies = true;
            spawnedPed.CanBeShotInVehicle = false;

            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;
            spawnedPed.Task.FightAgainstHatedTargets(400.0f);

            if (!Util.BlipIsOn(spawnedPed))
            {
                if (!Main.NoBlipOnCriminal) Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Tank, BlipColor.Red, "Terrorist " + spawnedVehicle.FriendlyName);

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
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUp(relationship);
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

                if (!Util.AnyEmergencyIsNear(spawnedPed.Position, DispatchManager.DispatchType.Army)) Main.DispatchAgainst(spawnedPed, type);
            }
        }
    }
}