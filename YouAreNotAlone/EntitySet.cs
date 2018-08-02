using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public abstract class EntitySet : AdvancedEntity
    {
        protected Ped spawnedPed;
        protected Vehicle spawnedVehicle;

        public EntitySet() { }

        protected bool SpawnedPedExistsIn(List<Ped> members)
        {
            if (Util.WeCanGiveTaskTo(spawnedPed)) return true;
            else
            {
                spawnedPed = null;

                if (Util.ThereIs(spawnedPed = members.Find(m => Util.WeCanGiveTaskTo(m))))
                {
                    Logger.Write(false, "EntitySet: Found driver.", "");

                    return true;
                }
                else
                {
                    Logger.Write(false, "EntitySet: Couldn't find driver. Need to be restored.", "");

                    return false;
                }
            }
        }

        protected bool ReadyToGoWith(List<Ped> members) => !Util.ThereIs(members.Find(p => Util.WeCanGiveTaskTo(p) && !p.IsSittingInVehicle(spawnedVehicle)));

        protected bool VehicleSeatsCanBeSeatedBy(List<Ped> members)
        {
            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Error("EntitySet: Vehicle doesn't exist. Abort to assign seats.", "");

                return false;
            }

            int startingSeat = 0;

            if (Util.WeCanGiveTaskTo(spawnedVehicle.Driver))
            {
                Logger.Write(false, "EntitySet: There is driver. Let it brake.", "");
                Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, spawnedVehicle.Driver, spawnedVehicle, 1, 1000);
            }
            else
            {
                Logger.Write(false, "EntitySet: No driver. Starts with driver seat.", "");
                startingSeat = -1;
            }

            for (int i = startingSeat, j = 0; j < members.Count; j++)
            {
                if (Util.WeCanGiveTaskTo(members[j]) && members[j].IsOnFoot && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, members[j], 195))
                {
                    while (!spawnedVehicle.IsSeatFree((VehicleSeat)i) && !spawnedVehicle.GetPedOnSeat((VehicleSeat)i).IsDead)
                    {
                        if (++i >= spawnedVehicle.PassengerSeats)
                        {
                            Logger.Error("EntitySet: Something wrong with assigning seats.", "");

                            return false;
                        }
                    }

                    members[j].Task.EnterVehicle(spawnedVehicle, (VehicleSeat)i++, 10000, 2.0f, 1);
                }
            }

            Logger.Write(false, "EntitySet: Assigned seats successfully.", "");

            return true;
        }
    }
}