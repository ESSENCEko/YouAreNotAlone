using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class SWATHeli : EmergencyHeli
    {
        public SWATHeli(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            spawnedVehicle = Util.Create(name, new Vector3(safePosition.X, safePosition.Y, safePosition.Z + 50.0f), (target.Position - safePosition).ToHeading(), false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            string selectedModel = models[Util.GetRandomInt(models.Count)];

            if (selectedModel == null) return false;

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModel));
                    Script.Wait(50);
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }

                p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                p.Weapons.Give(WeaponHash.SMG, 300, false, false);
                p.Weapons.Current.InfiniteAmmo = true;
                p.ShootRate = 700;

                p.Armor = 70;
                p.CanSwitchWeapons = true;
                Function.Call(Hash.SET_PED_ID_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_SEEING_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_HEARING_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_COMBAT_RANGE, p, 2);

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "COP");
            }

            spawnedVehicle.EngineRunning = true;
            spawnedVehicle.Livery = 0;
            Function.Call(Hash.SET_HELI_BLADES_FULL_SPEED, spawnedVehicle);

            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                foreach (Ped p in members)
                {
                    if (p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_HELI_PROTECT, p, spawnedVehicle, target, 50.0f, 32, 25.0f, 35, 1);
                    else p.Task.FightAgainstHatedTargets(100.0f);
                }
            }

            return true;
        }
    }
}