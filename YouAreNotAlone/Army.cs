using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Army : Emergency
    {
        public Army(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, (target.Position - position).ToHeading(), false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomInt(models.Count)]));
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
                p.Weapons.Give(WeaponHash.CombatMG, 500, false, false);
                p.Weapons.Current.InfiniteAmmo = true;
                p.ShootRate = 1000;

                p.Armor = 100;
                p.CanSwitchWeapons = true;
                Function.Call(Hash.SET_PED_ID_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_SEEING_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_HEARING_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_COMBAT_RANGE, p, 2);

                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "ARMY");
                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            spawnedVehicle.EngineRunning = true;

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver))
                {
                    Function.Call(Hash.SET_DRIVER_ABILITY, p, 1.0f);
                    Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, p, 1.0f);

                    if (((Ped)target).IsInVehicle()) Function.Call(Hash.TASK_VEHICLE_CHASE, p, target);
                    else p.Task.DriveTo(spawnedVehicle, target.Position, 30.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                }
                else p.Task.FightAgainstHatedTargets(100.0f);
            }

            return true;
        }
    }
}