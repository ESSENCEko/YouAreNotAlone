using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Firefighter : EmergencyFire
    {
        public Firefighter(string name, Entity target) : base(name, target, "FIREMAN") { Logger.ForceWrite(blipName + ": Time to put off fires.", this.name); }

        protected override void SetPedsOnDuty(bool onVehicleDuty)
        {
            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver) && Util.WeCanGiveTaskTo(spawnedVehicle.Driver))
                    {
                        Logger.Write(blipName + ": Time to go with vehicle.", name);

                        if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;

                        AddEmergencyBlip(true);
                        spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, targetPosition, 10.0f, 100.0f, 262708); // 4 + 16 + 32 + 512 + 262144
                    }
                    else
                    {
                        Logger.Write(blipName + ": There is no driver when on duty. Re-enter everyone.", name);

                        foreach (Ped p in members)
                        {
                            if (Util.ThereIs(p) && Util.WeCanGiveTaskTo(p) && p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        }
                    }
                }
                else
                {
                    if (!VehicleSeatsCanBeSeatedBy(members))
                    {
                        Logger.Write(blipName + ": Something wrong with assigning seats when on duty. Re-enter everyone.", name);

                        foreach (Ped p in members)
                        {
                            if (Util.ThereIs(p) && Util.WeCanGiveTaskTo(p) && p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        }
                    }
                    else Logger.Write(blipName + ": Assigned seats successfully when on duty.", name);
                }
            }
            else
            {
                Logger.Write(blipName + ": Time to put off fires.", name);
                AddEmergencyBlip(false);

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
            if (Util.ThereIs(target) && target.IsOnFire) return true;

            target = null;
            targetPosition = Vector3.Zero;
            OutputArgument outPos = new OutputArgument();

            if (Function.Call<bool>(Hash.GET_CLOSEST_FIRE_POS, outPos, spawnedVehicle.Position.X, spawnedVehicle.Position.Y, spawnedVehicle.Position.Z))
            {
                Vector3 position = outPos.GetResult<Vector3>();

                if (!position.Equals(Vector3.Zero) && spawnedVehicle.IsInRangeOf(position, 200.0f))
                {
                    Logger.Write(blipName + ": Found fire position.", name);
                    targetPosition = position;

                    return true;
                }
            }

            Logger.Write(blipName + ": Couldn't find fire position. Try to find entity on fire.", name);
            List<Entity> nearbyEntities = new List<Entity>(World.GetNearbyEntities(spawnedVehicle.Position, 200.0f));

            if (nearbyEntities.Count > 0)
            {
                Entity en = nearbyEntities.Find(e => Util.ThereIs(e) && e.IsOnFire);

                if (Util.ThereIs(en))
                {
                    Logger.Write(blipName + ": Found entity on fire.", name);
                    target = en;
                    targetPosition = target.Position;

                    return true;
                }
            }

            Logger.Write(blipName + ": There is no fire near.", name);

            return false;
        }
    }
}