using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Firefighter : EmergencyFire
    {
        public Firefighter(string name, Entity target) : base(name, target, "FIREMAN") { Logger.Write(true, blipName + ": Time to put off fires.", this.name); }

        protected override void SetPedsOnDuty(bool onVehicleDuty)
        {
            if (targetPosition.Equals(Vector3.Zero)) return;
            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.WeCanGiveTaskTo(spawnedVehicle.Driver))
                    {
                        Logger.Write(false, blipName + ": Time to go with vehicle.", name);

                        if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;

                        spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, targetPosition, 10.0f, 100.0f, 262708); // 4 + 16 + 32 + 512 + 262144
                    }
                    else
                    {
                        Logger.Write(false, blipName + ": There is no driver when on duty. Re-enter everyone.", name);

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                            p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
                else
                {
                    if (VehicleSeatsCanBeSeatedBy(members)) Logger.Write(false, blipName + ": Assigned seats successfully when on duty.", name);
                    else
                    {
                        Logger.Write(false, blipName + ": Something wrong with assigning seats when on duty. Re-enter everyone.", name);

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                            p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
            }
            else
            {
                if (spawnedVehicle.Speed < 1)
                {
                    if (Util.ThereIs(members.Find(p => Util.WeCanGiveTaskTo(p) && p.IsOnFoot && !p.Weapons.Current.Hash.Equals(WeaponHash.FireExtinguisher))))
                    {
                        foreach (Ped p in members.FindAll(p => Util.WeCanGiveTaskTo(p) && p.IsOnFoot && !p.Weapons.Current.Hash.Equals(WeaponHash.FireExtinguisher)))
                            p.Weapons.Select(WeaponHash.FireExtinguisher, true);
                    }
                    else
                    {
                        Logger.Write(false, blipName + ": Time to put off fires.", name);

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m)))
                        {
                            if (p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                            if (p.TaskSequenceProgress < 0)
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

                    if (Util.ThereIs(members.Find(p => Util.ThereIs(p) && p.TaskSequenceProgress == 1)))
                    {
                        Function.Call(Hash.STOP_FIRE_IN_RANGE, targetPosition.X, targetPosition.Y, targetPosition.Z, 3.0f);
                    }
                }
                else
                {
                    Logger.Write(false, blipName + ": Near fires. Time to brake.", name);

                    if (Util.WeCanGiveTaskTo(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, spawnedVehicle.Driver, spawnedVehicle, 1, 1000);
                }
            }
        }

        protected override bool TargetIsFound()
        {
            if (Util.ThereIs(target) && target.IsOnFire) return true;

            target = null;
            targetPosition = Vector3.Zero;

            if (Util.ThereIs(target = new List<Entity>(World.GetNearbyEntities(spawnedVehicle.Position, 200.0f)).Find(e => Util.ThereIs(e) && e.IsOnFire)))
            {
                Logger.Write(false, blipName + ": Found entity on fire.", name);
                targetPosition = target.Position;

                return true;
            }

            Logger.Write(false, blipName + ": Couldn't find entity on fire. Try to find fire position.", name);
            OutputArgument outPos = new OutputArgument();

            if (Function.Call<bool>(Hash.GET_CLOSEST_FIRE_POS, outPos, spawnedVehicle.Position.X, spawnedVehicle.Position.Y, spawnedVehicle.Position.Z))
            {
                Vector3 position = outPos.GetResult<Vector3>();

                if (!position.Equals(Vector3.Zero) && spawnedVehicle.IsInRangeOf(position, 200.0f))
                {
                    Logger.Write(false, blipName + ": Found fire position.", name);
                    targetPosition = position;

                    return true;
                }
            }

            Logger.Write(false, blipName + ": There is no fire near.", name);

            return false;
        }
    }
}