using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public abstract class EntitySet : AdvancedEntity
    {
        protected Ped spawnedPed;
        protected Vehicle spawnedVehicle;

        public EntitySet() { }

        protected bool ReadyToGoWith(List<Ped> members)
        {
            bool result = true;

            foreach (Ped p in members)
            {
                if (Util.ThereIs(p))
                {
                    if (!Util.WeCanGiveTaskTo(p))
                    {
                        if (p.Equals(spawnedVehicle.Driver) && spawnedVehicle.IsStopped && (!spawnedVehicle.Model.IsBicycle && !spawnedVehicle.Model.IsBicycle && !spawnedVehicle.Model.IsQuadbike))
                        {
                            Logger.Write("EntitySet: Driver is dead. Time to eject it.", "");
                            spawnedVehicle.OpenDoor(VehicleDoor.FrontLeftDoor, false, true);
                            Script.Wait(100);
                            Vector3 offset = p.Position + p.RightVector * (-1.01f);
                            p.Position = new Vector3(offset.X, offset.Y, offset.Z - 1.0f);
                        }
                    }
                    else if (!p.IsSittingInVehicle(spawnedVehicle)) result = false;
                }
            }

            if (result) Logger.Write("EntitySet: Ready to start.", "");
            else Logger.Write("EntitySet: Someone is not sitting in vehicle. Wait.", "");

            return result;
        }

        protected bool VehicleSeatsCanBeSeatedBy(List<Ped> members)
        {
            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Error("EntitySet: Vehicle doesn't exist. Abort to assign seats.", "");

                return false;
            }

            int startingSeat = 0;

            if (Util.ThereIs(spawnedVehicle.Driver) && Util.WeCanGiveTaskTo(spawnedVehicle.Driver))
            {
                Logger.Write("EntitySet: There is driver. Let it brake.", "");
                Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, spawnedVehicle.Driver, spawnedVehicle, 1, 1000);
            }
            else
            {
                Logger.Write("EntitySet: No driver. Starts with driver seat.", "");
                startingSeat = -1;
            }

            for (int i = startingSeat, j = 0; j < members.Count; j++)
            {
                if (Util.ThereIs(members[j]) && members[j].IsOnFoot && Util.WeCanGiveTaskTo(members[j]) && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, members[j], 195))
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

            Logger.Write("EntitySet: Assigned seats successfully.", "");

            return true;
        }
    }
}