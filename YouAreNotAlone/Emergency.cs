using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public abstract class Emergency : EntitySet
    {
        protected List<Ped> members;
        protected string name;
        protected Entity target;
        protected string emergencyType;
        protected string blipName;
        protected int relationship;

        public Emergency(string name, Entity target, string emergencyType) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
            this.target = target;
            this.emergencyType = emergencyType;
            this.blipName = "";

            if (this.emergencyType == "ARMY") this.relationship = Util.NewRelationshipOf(DispatchManager.DispatchType.Army);
            else this.relationship = Util.NewRelationshipOf(DispatchManager.DispatchType.Cop);
        }

        public abstract bool IsCreatedIn(Vector3 safePosition, List<string> models);
        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(blipName + ": Restore instantly.", name);

                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.Delete();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(blipName + ": Restore naturally.", name);

                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p) && p.IsPersistent)
                    {
                        p.AlwaysKeepTask = false;
                        p.BlockPermanentEvents = false;
                        Function.Call(Hash.SET_PED_AS_COP, p, true);
                        Util.NaturallyRemove(p);
                    }
                }

                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent)
                {
                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;

                    Util.NaturallyRemove(spawnedVehicle);
                }
            }

            if (relationship != 0)
            {
                if (emergencyType == "ARMY") Util.CleanUp(relationship, DispatchManager.DispatchType.Army);
                else Util.CleanUp(relationship, DispatchManager.DispatchType.Cop);
            }

            members.Clear();
        }

        protected void AddEmergencyBlip(bool onVehicle)
        {
            if (onVehicle)
            {
                Logger.Write(blipName + ": Members are in vehicle. Add blip on vehicle.", name);

                if (Util.WeCanEnter(spawnedVehicle))
                {
                    if (!Util.BlipIsOn(spawnedVehicle)) Util.AddBlipOn(spawnedVehicle, 0.5f, BlipSprite.PoliceOfficer, (BlipColor)(-1), blipName);
                }
                else if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.PoliceOfficer)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members)
                {
                    if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.PoliceOfficer)) p.CurrentBlip.Remove();
                }
            }
            else
            {
                Logger.Write(blipName + ": Members are on foot. Add blips on members.", name);

                if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.PoliceOfficer)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members)
                {
                    if (Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.4f, BlipSprite.PoliceOfficer, (BlipColor)(-1), blipName);
                    }
                    else if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.PoliceOfficer)) p.CurrentBlip.Remove();
                }
            }
        }

        protected void SetPedsOnDuty(bool onVehicleDuty)
        {
            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver))
                    {
                        Logger.Write(blipName + ": Time to fight in vehicle.", name);

                        if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;
                        if (!Main.NoBlipOnDispatch) AddEmergencyBlip(true);

                        foreach (Ped p in members)
                        {
                            if (Util.WeCanGiveTaskTo(p))
                            {
                                if (p.Equals(spawnedVehicle.Driver))
                                {
                                    if (target.Model.IsPed && ((Ped)target).IsInVehicle()) p.Task.VehicleChase((Ped)target);
                                    else p.Task.DriveTo(spawnedVehicle, target.Position, 10.0f, 100.0f, 262708); // 4 + 16 + 32 + 512 + 262144
                                }
                                else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                            }
                        }
                    }
                    else
                    {
                        Logger.Write(blipName + ": There is no driver when on duty. Re-enter everyone.", name);

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
                        Logger.Write(blipName + ": Something wrong with assigning seats when on duty. Re-enter everyone.", name);

                        foreach (Ped p in members)
                        {
                            if (Util.WeCanGiveTaskTo(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        }
                    }
                    else Logger.Write(blipName + ": Assigned seats successfully when on duty.", name);
                }
            }
            else
            {
                if (Util.ThereIs(spawnedVehicle.Driver) && Util.WeCanGiveTaskTo(spawnedVehicle.Driver))
                {
                    Logger.Write(blipName + ": Near the criminals. Time to get out of vehicle.", name);
                    TaskSequence ts = new TaskSequence();
                    Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, spawnedVehicle, 1, 1000);
                    ts.AddTask.LeaveVehicle(spawnedVehicle, false);
                    ts.AddTask.FightAgainstHatedTargets(400.0f);
                    ts.Close();

                    spawnedVehicle.Driver.Task.PerformSequence(ts);
                    ts.Dispose();
                }
                else
                {
                    Logger.Write(blipName + ": Time to fight on foot.", name);

                    if (!Main.NoBlipOnDispatch) AddEmergencyBlip(false);

                    foreach (Ped p in members)
                    {
                        if (!p.IsInCombat && Util.WeCanGiveTaskTo(p)) p.Task.FightAgainstHatedTargets(400.0f);
                    }
                }
            }
        }

        protected void SetPedsOffDuty()
        {
            if (!Util.WeCanEnter(spawnedVehicle)) Restore(false);
            else if (ReadyToGoWith(members))
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    Logger.Write(blipName + ": Time to be off duty.", name);

                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;
                    if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.PoliceOfficer)) spawnedVehicle.CurrentBlip.Remove();
                    if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, spawnedVehicle.Driver, 151))
                    {
                        foreach (Ped p in members)
                        {
                            if (Util.ThereIs(p))
                            {
                                if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.PoliceOfficer)) p.CurrentBlip.Remove();
                                if (Util.WeCanGiveTaskTo(p))
                                {
                                    if (p.Equals(spawnedVehicle.Driver)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                                    else p.Task.Wait(1000);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger.Write(blipName + ": There is no driver when off duty. Re-enter everyone.", name);

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
                    Logger.Write(blipName + ": Something wrong with assigning seats when off duty. Re-enter everyone.", name);

                    foreach (Ped p in members)
                    {
                        if (Util.WeCanGiveTaskTo(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
                else Logger.Write(blipName + ": Assigned seats successfully when off duty.", name);
            }
        }

        protected bool TargetIsFound()
        {
            target = null;
            Ped[] nearbyPeds = World.GetNearbyPeds(spawnedVehicle.Position, 300.0f);

            if (nearbyPeds.Length < 1)
            {
                Logger.Write(blipName + ": There is no peds nearby. Abort finding target.", name);

                return false;
            }

            foreach (Ped p in nearbyPeds)
            {
                if (Util.ThereIs(p) && !p.IsDead && World.GetRelationshipBetweenGroups(relationship, p.RelationshipGroup).Equals(Relationship.Hate))
                {
                    Logger.Write(blipName + ": Found target.", name);
                    target = p;

                    return true;
                }
            }

            Logger.Write(blipName + ": Couldn't find target.", name);

            return false;
        }

        protected void AddVarietyTo(Ped p)
        {
            if (Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 0, 0) > 0
                && Util.GetRandomIntBelow(2) == 1)
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, Util.GetRandomIntBelow(Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 0, 0)), 0, false);

            if (Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 1, 0) > 0
                && Util.GetRandomIntBelow(2) == 1)
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, Util.GetRandomIntBelow(Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 1, 0)), 0, false);
        }

        public override bool ShouldBeRemoved()
        {
            int alive = 0;

            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);

                    continue;
                }

                if (Util.WeCanGiveTaskTo(members[i])) alive++;
                else if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
            }

            Logger.Write(blipName + ": Alive members - " + alive.ToString(), name);

            if (!Util.ThereIs(spawnedVehicle) || alive < 1 || members.Count < 1)
            {
                Logger.Write(blipName + ": Emergency need to be restored.", name);
                Restore(false);

                return true;
            }

            if (!TargetIsFound())
            {
                if (!spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
                {
                    Logger.Write(blipName + ": Target not found and too far from player. Time to be restored.", name);
                    Restore(false);

                    return true;
                }
                else
                {
                    Logger.Write(blipName + ": Target not found. Time to be off duty.", name);
                    SetPedsOffDuty();
                }
            }
            else
            {
                if (!spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    Logger.Write(blipName + ": Target found but too far from player. Time to be restored.", name);
                    Restore(false);

                    return true;
                }
                else
                {
                    Logger.Write(blipName + ": Target found. Time to be on duty.", name);
                    SetPedsOnDuty(Util.WeCanEnter(spawnedVehicle) && (!spawnedVehicle.IsInRangeOf(target.Position, 30.0f) || (target.Model.IsPed && ((Ped)target).IsInVehicle() && ((Ped)target).CurrentVehicle.Speed > 10.0f)));
                }
            }

            return false;
        }
    }
}