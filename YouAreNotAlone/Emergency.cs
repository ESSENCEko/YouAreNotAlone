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
        protected bool offDuty;
        protected TaskSequence ts;

        public Emergency(string name, Entity target, string emergencyType) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
            this.target = target;
            this.emergencyType = emergencyType;
            this.blipName = "";
            this.offDuty = false;
            this.ts = null;

            if (this.emergencyType == "ARMY") this.relationship = Util.NewRelationshipOf(DispatchManager.DispatchType.ArmyGround);
            else this.relationship = Util.NewRelationshipOf(DispatchManager.DispatchType.CopGround);
        }

        public abstract bool IsCreatedIn(Vector3 safePosition, List<string> models);

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, blipName + ": Restore instantly.", name);

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m))) p.Delete();

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(false, blipName + ": Restore naturally.", name);

                foreach (Ped p in members) Util.NaturallyRemove(p);

                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;

                Util.NaturallyRemove(spawnedVehicle);
            }

            if (ts != null) ts.Dispose();
            if (relationship != 0)
            {
                if (emergencyType == "ARMY") Util.CleanUp(relationship, DispatchManager.DispatchType.ArmyGround);
                else Util.CleanUp(relationship, DispatchManager.DispatchType.CopGround);
            }

            members.Clear();
        }

        protected abstract BlipSprite CurrentBlipSprite { get; }

        protected bool TaskIsSet()
        {
            if (ts == null)
            {
                ts = new TaskSequence();
                ts.AddTask.LeaveVehicle(spawnedVehicle, false);
                ts.AddTask.FightAgainstHatedTargets(400.0f);
                ts.Close();

                return true;
            }
            else return false;
        }

        protected void SetPedsOnDuty(bool onVehicleDuty)
        {
            if (!Util.ThereIs(target) || !target.Model.IsPed)
            {
                target = null;

                return;
            }

            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver))
                    {
                        Logger.Write(false, blipName + ": Time to fight in vehicle.", name);

                        if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m)))
                        {
                            if (p.Equals(spawnedVehicle.Driver))
                            {
                                if (this.GetType().Equals(typeof(EmergencyHeli))) Function.Call(Hash.TASK_VEHICLE_HELI_PROTECT, p, spawnedVehicle, target, 50.0f, 32, 25.0f, 35, 1);
                                else
                                {
                                    if (((Ped)target).IsInVehicle()) p.Task.VehicleChase((Ped)target);
                                    else p.Task.DriveTo(spawnedVehicle, target.Position, 10.0f, 100.0f, 262708); // 4 + 16 + 32 + 512 + 262144
                                }
                            }
                            else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                        }
                    }
                    else if (!this.GetType().Equals(typeof(EmergencyHeli)) || !spawnedVehicle.IsInAir)
                    {
                        Logger.Write(false, blipName + ": There is no driver when on duty. Re-enter everyone.", name);

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                            p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
                else if (!this.GetType().Equals(typeof(EmergencyHeli)) || !spawnedVehicle.IsInAir)
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
                if (this.GetType().Equals(typeof(EmergencyHeli)))
                {
                    foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m)))
                    {
                        if (p.IsSittingInVehicle(spawnedVehicle)) p.Task.PerformSequence(ts);
                        else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                    }
                }
                else
                {
                    if (spawnedVehicle.Speed < 1)
                    {
                        Logger.Write(false, blipName + ": Time to fight on foot.", name);

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m)))
                        {
                            if (p.IsSittingInVehicle(spawnedVehicle))
                            {
                                OutputArgument weaponHash = new OutputArgument();

                                if (!Function.Call<bool>(Hash.GET_CURRENT_PED_VEHICLE_WEAPON, p, weaponHash) || weaponHash.GetResult<int>() == 1422046295) p.Task.PerformSequence(ts);
                                else p.Task.VehicleShootAtPed((Ped)target);
                            }
                            else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                        }
                    }
                    else
                    {
                        Logger.Write(false, blipName + ": Near the criminals. Time to brake.", name);

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m)))
                        {
                            if (p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, p, spawnedVehicle, 1, 1000);
                            else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                        }
                    }
                }
            }
        }

        protected void SetPedsOffDuty()
        {
            if (offDuty)
            {
                if (!Util.WeCanEnter(spawnedVehicle))
                {
                    foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m) && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, m, 100)))
                        p.Task.WanderAround();
                }
                else if (!ReadyToGoWith(members)) offDuty = false;
            }
            else
            {
                if (!Util.WeCanEnter(spawnedVehicle)) offDuty = true;
                else if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver))
                    {
                        Logger.Write(false, blipName + ": Time to be off duty.", name);

                        if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m)))
                        {
                            if (p.Equals(spawnedVehicle.Driver))
                            {
                                if (!this.GetType().Equals(typeof(EmergencyHeli))) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                            }
                            else p.Task.Wait(1000);
                        }

                        offDuty = true;
                    }
                    else if (!this.GetType().Equals(typeof(EmergencyHeli)) || !spawnedVehicle.IsInAir)
                    {
                        Logger.Write(false, blipName + ": There is no driver when off duty. Re-enter everyone.", name);

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                            p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
                else if (!this.GetType().Equals(typeof(EmergencyHeli)) || !spawnedVehicle.IsInAir)
                {
                    if (VehicleSeatsCanBeSeatedBy(members)) Logger.Write(false, blipName + ": Assigned seats successfully when off duty.", name);
                    else
                    {
                        Logger.Write(false, blipName + ": Something wrong with assigning seats when off duty. Re-enter everyone.", name);

                        foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                            p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
            }
        }

        protected bool TargetIsFound()
        {
            if (Util.WeCanGiveTaskTo((Ped)target) && spawnedVehicle.IsInRangeOf(target.Position, 150.0f) && World.GetRelationshipBetweenGroups(relationship, ((Ped)target).RelationshipGroup).Equals(Relationship.Hate)) return true;

            target = null;
            List<Ped> nearbyPeds = new List<Ped>(World.GetNearbyPeds(spawnedVehicle.Position, 300.0f));

            if (Util.ThereIs(target = nearbyPeds.Find(p => Util.WeCanGiveTaskTo(p) && World.GetRelationshipBetweenGroups(relationship, p.RelationshipGroup).Equals(Relationship.Hate))))
            {
                Logger.Write(false, blipName + ": Found target.", name);

                return true;
            }

            if (Util.ThereIs(target = emergencyType == "ARMY" ?
                nearbyPeds.Find(p => Util.WeCanGiveTaskTo(p) && p.IsInCombat && p.IsSittingInVehicle() && Function.Call<bool>(Hash.DOES_VEHICLE_HAVE_WEAPONS, p.CurrentVehicle) && World.GetRelationshipBetweenGroups(relationship, p.RelationshipGroup) > Relationship.Like) :
                nearbyPeds.Find(p => Util.WeCanGiveTaskTo(p) && p.IsInCombat && World.GetRelationshipBetweenGroups(relationship, p.RelationshipGroup) > Relationship.Like)))
            {
                Logger.Write(false, blipName + ": Found new target.", name);
                Util.SetAsCriminalWhoIs((Ped)target, emergencyType);

                return true;
            }

            Logger.Write(false, blipName + ": Couldn't find target.", name);

            return false;
        }

        protected void AddVarietyTo(Ped p)
        {
            if (!Util.ThereIs(p)) return;

            int n = 0;

            if ((n = Function.Call<int>(Hash.GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS, p, 8)) > 0 && Util.GetRandomIntBelow(2) == 1)
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 8, Util.GetRandomIntBelow(n), 0, 0);

            if ((n = Function.Call<int>(Hash.GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS, p, 9)) > 0 && Util.GetRandomIntBelow(2) == 1)
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 9, Util.GetRandomIntBelow(n), 0, 0);

            if ((n = Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_DRAWABLE_VARIATIONS, p, 0)) > 0 && Util.GetRandomIntBelow(2) == 1)
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, Util.GetRandomIntBelow(n), 0, false);

            if ((n = Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_DRAWABLE_VARIATIONS, p, 1)) > 0 && Util.GetRandomIntBelow(2) == 1)
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, Util.GetRandomIntBelow(n), 0, false);
        }

        protected void RefreshBlip(bool forceOff)
        {
            if (forceOff)
            {
                if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members.FindAll(m => Util.BlipIsOn(m))) p.CurrentBlip.Remove();
            }
            else if (ReadyToGoWith(members))
            {
                if (Util.WeCanEnter(spawnedVehicle))
                {
                    if (!Util.BlipIsOn(spawnedVehicle)) Util.AddEmergencyBlipOn(spawnedVehicle, CurrentBlipSprite.Equals(BlipSprite.PoliceOfficer) ? 0.5f : 0.7f, CurrentBlipSprite, blipName);
                }
                else if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members.FindAll(m => Util.BlipIsOn(m))) p.CurrentBlip.Remove();
            }
            else
            {
                if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m)))
                {
                    if (Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddEmergencyBlipOn(p, CurrentBlipSprite.Equals(BlipSprite.PoliceOfficer) ? 0.4f : 0.5f, CurrentBlipSprite, blipName);
                    }
                    else if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();
                }
            }
        }

        public override bool ShouldBeRemoved()
        {
            int alive = 0;

            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (Util.WeCanGiveTaskTo(members[i])) alive++;
                else
                {
                    Util.NaturallyRemove(members[i]);
                    members.RemoveAt(i);
                }
            }

            Logger.Write(false, blipName + ": Alive members - " + alive.ToString(), name);

            if (!Util.ThereIs(spawnedVehicle) || alive < 1 || members.Count < 1)
            {
                Logger.Write(false, blipName + ": Emergency need to be restored.", name);

                return true;
            }

            if (TargetIsFound())
            {
                if (offDuty) offDuty = false;
                if (spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    Logger.Write(false, blipName + ": Target found. Time to be on duty.", name);
                    SetPedsOnDuty(Util.WeCanEnter(spawnedVehicle) && (!spawnedVehicle.IsInRangeOf(target.Position, 30.0f) || (target.Model.IsPed && ((Ped)target).IsInVehicle() && ((Ped)target).CurrentVehicle.Speed > 10.0f)));
                    RefreshBlip(false);
                }
                else
                {
                    Logger.Write(false, blipName + ": Target found but too far from player. Time to be restored.", name);

                    return true;
                }
            }
            else
            {
                if (spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
                {
                    Logger.Write(false, blipName + ": Target not found. Time to be off duty.", name);
                    SetPedsOffDuty();
                    RefreshBlip(true);
                }
                else
                {
                    Logger.Write(false, blipName + ": Target not found and too far from player. Time to be restored.", name);

                    return true;
                }
            }

            return false;
        }
    }
}