﻿using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class EmergencyBlock : Emergency
    {
        public EmergencyBlock(string name, Entity target, string emergencyType) : base(name, target, emergencyType)
        {
            this.blipName += emergencyType + " Road Block";
            Logger.Write(true, blipName + ": Time to block road.", this.name);
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            if (relationship == 0) return false;

            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                if (!road.Position.Equals(Vector3.Zero))
                {
                    Logger.Write(false, blipName + ": Found proper road.", name);

                    break;
                }
            }

            if (road.Position.Equals(Vector3.Zero))
            {
                Logger.Error(blipName + ": Couldn't find proper road. Abort.", name);

                return false;
            }

            spawnedVehicle = Util.Create(name, road.Position, road.Heading + 90, false);

            if (!Util.ThereIs(spawnedVehicle) || !TaskIsSet())
            {
                Logger.Error(blipName + ": Couldn't create vehicle. Abort.", name);

                return false;
            }

            Logger.Write(false, blipName + ": Tried to create stinger and members.", name);

            Stinger s = new Stinger(spawnedVehicle);

            if (s.IsCreatedIn(spawnedVehicle.Position - spawnedVehicle.ForwardVector * spawnedVehicle.Model.GetDimensions().Y) && DispatchManager.Add(s, DispatchManager.DispatchType.Stinger))
                Logger.Write(false, blipName + ": Created stinger.", name);
            else s.Restore(true);

            if (emergencyType == "LSPD")
            {
                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 1; i++)
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

                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 1; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                    {
                        members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModel));
                        Script.Wait(50);
                    }
                }
            }

            if (members.Find(p => !Util.ThereIs(p)) != null)
            {
                Logger.Error(blipName + ": There is a member who doesn't exist. Abort.", name);
                Restore(true);

                return false;
            }

            foreach (Ped p in members)
            {
                AddVarietyTo(p);
                Util.SetCombatAttributesOf(p);

                Function.Call(Hash.SET_DRIVER_ABILITY, p, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, p, 1.0f);
                Function.Call(Hash.SET_PED_AS_COP, p, false);

                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.FiringPattern = FiringPattern.BurstFireDriveby;

                if (p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, LeaveVehicleFlags.WarpOut);

                switch (emergencyType)
                {
                    case "ARMY":
                        {
                            p.Weapons.Give(WeaponHash.CombatMG, 500, false, true);
                            p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                            p.ShootRate = 1000;
                            p.Armor = 100;

                            break;
                        }

                    case "LSPD":
                        {
                            p.Weapons.Give(WeaponHash.PumpShotgun, 30, false, true);
                            p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                            p.ShootRate = 500;
                            p.Armor = 30;

                            break;
                        }

                    case "SWAT":
                        {
                            p.Weapons.Give(WeaponHash.Pistol, 100, false, true);
                            p.Weapons.Give(WeaponHash.SMG, 300, true, true);
                            p.ShootRate = 700;
                            p.Armor = 70;

                            break;
                        }
                }

                p.Weapons.Current.InfiniteAmmo = true;
                p.CanSwitchWeapons = true;
                p.RelationshipGroup = relationship;
                Logger.Write(false, blipName + ": Characteristics are set.", name);
            }

            spawnedVehicle.EngineRunning = true;
            Logger.Write(false, blipName + ": Ready to dispatch.", name);

            return true;
        }

        protected override BlipSprite CurrentBlipSprite => BlipSprite.PoliceOfficer;
    }
}