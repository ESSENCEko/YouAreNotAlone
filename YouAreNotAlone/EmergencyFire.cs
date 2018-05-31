using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public abstract class EmergencyFire : Emergency
    {
        protected Vector3 targetPosition;

        public EmergencyFire(string name, Entity target, string emergencyType) : base(name, target, emergencyType)
        {
            this.blipName += emergencyType == "FIREMAN" ? "Fire Fighter" : "Paramedic";
            Util.CleanUp(this.relationship, DispatchManager.DispatchType.Cop);
            this.relationship = 0;
            this.targetPosition = target.Position;
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                if (!road.Position.Equals(Vector3.Zero))
                {
                    Logger.Write(blipName + ": Found proper road.", name);

                    break;
                }
            }

            if (road.Position.Equals(Vector3.Zero))
            {
                Logger.Error(blipName + ": Couldn't find proper road. Abort.", name);

                return false;
            }

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, false);

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Write(blipName + ": Couldn't create vehicle. Abort.", name);

                return false;
            }

            int max = emergencyType == "FIREMAN" ? 3 : 1;

            for (int i = -1; i < spawnedVehicle.PassengerSeats && i < max; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomIntBelow(models.Count)]));
                    Script.Wait(50);
                }
            }

            Logger.Write(blipName + ": Created members.", name);

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Logger.Error(blipName + ": There is a member who doesn't exist. Abort.", name);
                    Restore(true);

                    return false;
                }

                if (emergencyType == "FIREMAN")
                {
                    p.Weapons.Give(WeaponHash.FireExtinguisher, 100, true, true);
                    p.Weapons.Current.InfiniteAmmo = true;
                    p.CanSwitchWeapons = true;
                    p.IsFireProof = true;
                }

                AddVarietyTo(p);
                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                Logger.Write(blipName + ": Characteristics are set.", name);
            }

            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                Function.Call(Hash.SET_DRIVER_ABILITY, spawnedVehicle.Driver, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedVehicle.Driver, 1.0f);
            }

            spawnedVehicle.EngineRunning = true;
            SetPedsOnDuty(true);
            Logger.Write(blipName + ": Ready to dispatch.", name);

            return true;
        }

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
                        Util.NaturallyRemove(p);
                    }
                }

                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent)
                {
                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;

                    Util.NaturallyRemove(spawnedVehicle);
                }
            }

            members.Clear();
        }

        protected new void AddEmergencyBlip(bool onVehicle)
        {
            if (onVehicle)
            {
                Logger.Write(blipName + ": Members are in vehicle. Add blip on vehicle.", name);

                if (Util.WeCanEnter(spawnedVehicle))
                {
                    if (!Util.BlipIsOn(spawnedVehicle)) Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.Hospital, BlipColor.Red, blipName);
                }
                else if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members)
                {
                    if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) p.CurrentBlip.Remove();
                }
            }
            else
            {
                Logger.Write(blipName + ": Members are on foot. Add blips on members.", name);

                if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members)
                {
                    if (Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.5f, BlipSprite.Hospital, BlipColor.Red, blipName);
                    }
                    else if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) p.CurrentBlip.Remove();
                }
            }
        }

        protected new abstract void SetPedsOnDuty(bool onVehicleDuty);
        protected new void SetPedsOffDuty()
        {
            if (!Util.WeCanEnter(spawnedVehicle)) Restore(false);
            else if (ReadyToGoWith(members))
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    Logger.Write(blipName + ": Time to be off duty.", name);

                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;
                    if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) spawnedVehicle.CurrentBlip.Remove();
                    if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, spawnedVehicle.Driver, 151))
                    {
                        foreach (Ped p in members)
                        {
                            if (Util.ThereIs(p))
                            {
                                if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) p.CurrentBlip.Remove();
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

        protected abstract new bool TargetIsFound();
        private new void AddVarietyTo(Ped p)
        {
            if (emergencyType == "FIREMAN")
            {
                switch (Util.GetRandomIntBelow(3))
                {
                    case 1:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, 0, 0, false);
                        break;

                    case 2:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 0, 0, false);
                        break;
                }
            }
            else
            {
                switch (Util.GetRandomIntBelow(4))
                {
                    case 1:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, 0, 0, false);
                        break;

                    case 2:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 0, 0, false);
                        break;

                    case 3:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, 0, 0, false);
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 0, 0, false);
                        break;
                }
            }
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

            if (!Util.ThereIs(spawnedVehicle) || !Util.WeCanEnter(spawnedVehicle) || alive < 1 || members.Count < 1)
            {
                Logger.Write(blipName + ": Emergency fire need to be restored.", name);
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
                    SetPedsOnDuty(!spawnedVehicle.IsInRangeOf(targetPosition, 30.0f));
                }
            }

            return false;
        }
    }
}