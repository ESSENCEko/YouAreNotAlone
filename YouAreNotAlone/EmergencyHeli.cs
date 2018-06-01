using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class EmergencyHeli : Emergency
    {
        public EmergencyHeli(string name, Entity target, string emergencyType) : base(name, target, emergencyType)
        {
            this.blipName += emergencyType + " Helicopter";
            Logger.Write(blipName + ": Time to dispatch.", name);
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Vector3 position = World.GetNextPositionOnStreet(safePosition.Around(10.0f));

            if (position.Equals(Vector3.Zero)) position = safePosition;

            spawnedVehicle = Util.Create(name, new Vector3(position.X, position.Y, position.Z + 50.0f), (target.Position - position).ToHeading(), false);

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Error(blipName + ": Couldn't create vehicle. Abort.", name);

                return false;
            }

            if (emergencyType == "LSPD")
            {
                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 3; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                    {
                        members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomIntBelow(models.Count)]));
                        Script.Wait(50);
                    }
                }
            }
            else
            {
                string selectedModel = models[Util.GetRandomIntBelow(models.Count)];

                if (selectedModel == null)
                {
                    Logger.Error(blipName + ": Couldn't find model. Abort.", name);
                    Restore(true);

                    return false;
                }

                for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                    {
                        members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModel));
                        Script.Wait(50);
                    }
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

                switch (emergencyType)
                {
                    case "ARMY":
                        {
                            p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                            p.Weapons.Give(WeaponHash.CombatMG, 500, false, false);
                            p.ShootRate = 1000;
                            p.Armor = 100;

                            break;
                        }

                    case "LSPD":
                        {
                            p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                            p.ShootRate = 500;
                            p.Armor = 30;

                            break;
                        }

                    case "SWAT":
                        {
                            p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                            p.Weapons.Give(WeaponHash.SMG, 300, false, false);
                            p.ShootRate = 700;
                            p.Armor = 70;

                            break;
                        }
                }

                p.Weapons.Current.InfiniteAmmo = true;
                p.CanSwitchWeapons = true;

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 17, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                p.RelationshipGroup = relationship;
                p.NeverLeavesGroup = true;
                Logger.Write(blipName + ": Characteristics are set.", name);
            }

            spawnedVehicle.EngineRunning = true;
            spawnedVehicle.Livery = 0;
            spawnedVehicle.PrimaryColor = VehicleColor.MetallicBlack;
            spawnedVehicle.SecondaryColor = VehicleColor.MetallicBlack;
            Function.Call(Hash.SET_HELI_BLADES_FULL_SPEED, spawnedVehicle);
            SetPedsOnDuty(true);
            Logger.Write(blipName + ": Ready to dispatch.", name);

            return true;
        }

        private new void AddEmergencyBlip(bool onVehicle)
        {
            if (onVehicle)
            {
                Logger.Write(blipName + ": Members are in vehicle. Add blip on vehicle.", name);

                if (Util.WeCanEnter(spawnedVehicle))
                {
                    if (!Util.BlipIsOn(spawnedVehicle)) Util.AddEmergencyBlipOn(spawnedVehicle, 0.7f, BlipSprite.PoliceHelicopterAnimated, blipName);
                }
                else if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.PoliceHelicopterAnimated)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members)
                {
                    if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.PoliceOfficer)) p.CurrentBlip.Remove();
                }
            }
            else
            {
                Logger.Write(blipName + ": Members are on foot. Add blips on members.", name);

                if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.PoliceHelicopterAnimated)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members)
                {
                    if (Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddEmergencyBlipOn(p, 0.4f, BlipSprite.PoliceOfficer, blipName);
                    }
                    else if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.PoliceOfficer)) p.CurrentBlip.Remove();
                }
            }
        }

        private new void SetPedsOnDuty(bool onVehicleDuty)
        {
            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver))
                    {
                        Logger.Write(blipName + ": Time to fight in vehicle.", name);

                        if (!Main.NoBlipOnDispatch) AddEmergencyBlip(true);

                        foreach (Ped p in members)
                        {
                            if (Util.ThereIs(p) && Util.WeCanGiveTaskTo(p))
                            {
                                if (p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_HELI_PROTECT, p, spawnedVehicle, target, 50.0f, 32, 25.0f, 35, 1);
                                else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                            }
                        }
                    }
                    else if (spawnedVehicle.IsOnAllWheels)
                    {
                        Logger.Write(blipName + ": Time to fight in vehicle.", name);

                        foreach (Ped p in members)
                        {
                            if (Util.WeCanGiveTaskTo(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        }
                    }
                }
                else if (spawnedVehicle.IsOnAllWheels)
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
                Logger.Write(blipName + ": Time to fight on foot.", name);

                if (!Main.NoBlipOnDispatch) AddEmergencyBlip(false);

                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p) && Util.WeCanGiveTaskTo(p))
                    {
                        if (p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                    }
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

                if (Util.WeCanGiveTaskTo(members[i]))
                {
                    if (!members[i].Equals(spawnedVehicle.Driver)) alive++;
                }
                else if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
            }

            Logger.Write(blipName + ": Alive members without driver - " + alive.ToString(), name);

            if (!Util.ThereIs(spawnedVehicle) || !TargetIsFound() || alive < 1 || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(blipName + ": Emergency helicopter need to be restored.", name);
                Restore(false);

                return true;
            }
            else
            {
                Logger.Write(blipName + ": Found target. Time to be on duty.", name);
                SetPedsOnDuty(Util.WeCanEnter(spawnedVehicle) || !spawnedVehicle.IsOnAllWheels);
            }

            return false;
        }
    }
}