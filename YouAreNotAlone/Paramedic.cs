using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Paramedic : EmergencyFire
    {
        private List<int> checkedPeds;
        private List<string> scenarios;

        public Paramedic(string name, Entity target) : base(name, target, "MEDIC")
        {
            this.checkedPeds = new List<int>();
            this.scenarios = new List<string>
            {
                "CODE_HUMAN_MEDIC_KNEEL",
                "CODE_HUMAN_MEDIC_TEND_TO_DEAD",
                "CODE_HUMAN_MEDIC_TIME_OF_DEATH"
            };
            Logger.Write(true, blipName + ": Time to investigate dead bodies.", this.name);
        }

        protected override void SetPedsOnDuty(bool onVehicleDuty)
        {
            if (!Util.ThereIs(target) || !target.Model.IsPed)
            {
                target = null;
                targetPosition = Vector3.Zero;

                return;
            }

            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver) && Util.WeCanGiveTaskTo(spawnedVehicle.Driver))
                    {
                        Logger.Write(false, blipName + ": Time to go with vehicle.", name);

                        if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;

                        spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, targetPosition, 10.0f, 100.0f, 262708); // 4 + 16 + 32 + 512 + 262144
                    }
                    else
                    {
                        Logger.Write(false, blipName + ": There is no driver when on duty. Re-enter everyone.", name);

                        foreach (Ped p in members.FindAll(m => Util.ThereIs(m) && Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                            p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
                else
                {
                    if (VehicleSeatsCanBeSeatedBy(members)) Logger.Write(false, blipName + ": Assigned seats successfully when on duty.", name);
                    else
                    {
                        Logger.Write(false, blipName + ": Something wrong with assigning seats when on duty. Re-enter everyone.", name);

                        foreach (Ped p in members.FindAll(m => Util.ThereIs(m) && Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                            p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
            }
            else
            {
                if (spawnedVehicle.Speed < 1)
                {
                    if ((!Util.ThereIs(members.Find(p => Util.ThereIs(p) && p.TaskSequenceProgress < 2)) || Util.ThereIs(members.Find(p => Util.ThereIs(p) && p.TaskSequenceProgress == 3))) && !checkedPeds.Contains(target.Handle))
                    {
                        Logger.Write(false, blipName + ": A dead body is checked.", name);
                        checkedPeds.Add(target.Handle);
                    }
                    else
                    {
                        Logger.Write(false, blipName + ": Time to investigate dead bodies.", name);

                        foreach (Ped p in members.FindAll(m => Util.ThereIs(m) && Util.WeCanGiveTaskTo(m)))
                        {
                            if (p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                            if (p.TaskSequenceProgress < 0)
                            {
                                Vector3 dest = targetPosition.Around(1.0f);

                                TaskSequence ts = new TaskSequence();
                                ts.AddTask.RunTo(dest);
                                ts.AddTask.AchieveHeading((targetPosition - dest).ToHeading());
                                Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, 0, scenarios[Util.GetRandomIntBelow(scenarios.Count)], 0, 1);
                                ts.AddTask.Wait(1000);
                                ts.Close();

                                p.Task.PerformSequence(ts);
                                ts.Dispose();
                            }
                        }
                    }
                }
                else
                {
                    Logger.Write(false, blipName + ": Near dead bodies. Time to brake.", name);

                    if (Util.ThereIs(spawnedVehicle.Driver) && Util.WeCanGiveTaskTo(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, spawnedVehicle.Driver, spawnedVehicle, 1, 1000);
                }
            }
        }

        protected override bool TargetIsFound()
        {
            if (Util.ThereIs(target) && target.Model.IsPed && !checkedPeds.Contains(target.Handle)) return true;

            target = null;
            targetPosition = Vector3.Zero;

            if (Util.ThereIs(target = new List<Ped>(World.GetNearbyPeds(spawnedVehicle.Position, 200.0f)).Find(p => Util.ThereIs(p) && p.IsDead && !checkedPeds.Contains(p.Handle))))
            {
                Logger.Write(false, blipName + ": Found a dead body.", name);
                targetPosition = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, (Ped)target, 11816, 0.0f, 0.0f, 0.0f);

                return true;
            }

            Logger.Write(false, blipName + ": There is no dead body.", name);

            return false;
        }
    }
}