using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Firefighter : EmergencyFire
    {
        public Firefighter(string name, Entity target) : base(name, target, "FIREMAN") { }

        protected override void SetPedsOnDuty()
        {
            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver) && Util.WeCanGiveTaskTo(spawnedVehicle.Driver))
                    {
                        if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;
                        if (!Main.NoBlipOnDispatch) AddEmergencyBlip(true);

                        spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, targetPosition, 10.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                    }
                    else
                    {
                        foreach (Ped p in members)
                        {
                            if (Util.WeCanGiveTaskTo(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        }
                    }
                }
                else
                {
                    if (!VehicleSeatsCanBeSeatedBy(members))
                    {
                        Restore(false);
                        return;
                    }
                }
            }
            else
            {
                if (!Main.NoBlipOnDispatch) AddEmergencyBlip(false);

                foreach (Ped p in members)
                {
                    if (p.TaskSequenceProgress < 0 && Util.WeCanGiveTaskTo(p))
                    {
                        TaskSequence ts = new TaskSequence();
                        ts.AddTask.RunTo(targetPosition.Around(3.0f));
                        ts.AddTask.ShootAt(targetPosition, 10000, FiringPattern.FullAuto);
                        ts.Close();

                        p.Task.PerformSequence(ts);
                        ts.Dispose();
                    }
                }
            }
        }

        protected override bool TargetIsFound()
        {
            target = null;
            targetPosition = Vector3.Zero;
            OutputArgument outPos = new OutputArgument();

            if (Function.Call<bool>(Hash.GET_CLOSEST_FIRE_POS, outPos, spawnedVehicle.Position.X, spawnedVehicle.Position.Y, spawnedVehicle.Position.Z))
            {
                Vector3 position = outPos.GetResult<Vector3>();

                if (!position.Equals(Vector3.Zero) && spawnedVehicle.IsInRangeOf(position, 200.0f))
                {
                    targetPosition = position;
                    return true;
                }
            }

            Entity[] nearbyEntities = World.GetNearbyEntities(spawnedVehicle.Position, 200.0f);

            if (nearbyEntities.Length < 1) return false;

            foreach (Entity en in nearbyEntities)
            {
                if (Util.ThereIs(en) && en.IsOnFire)
                {
                    target = en;
                    targetPosition = target.Position;
                    return true;
                }
            }

            return false;
        }
    }
}