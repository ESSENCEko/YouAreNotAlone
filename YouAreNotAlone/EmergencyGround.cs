using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class EmergencyGround : Emergency
    {
        public EmergencyGround(string name, Entity target, string emergencyType) : base(name, target, emergencyType)
        {
            this.blipName += emergencyType + " Ground";
            Logger.Write(true, blipName + ": Time to dispatch.", this.name);
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            if (relationship == 0) return false;

            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeadingToChase(safePosition.Around(50.0f), target.Position);

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

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, false);

            if (!Util.ThereIs(spawnedVehicle) || !TaskIsSet())
            {
                Logger.Error(blipName + ": Couldn't create vehicle. Abort.", name);

                return false;
            }

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

                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 5; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                    {
                        members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModel));
                        Script.Wait(50);
                    }
                }
            }

            Logger.Write(false, blipName + ": Created members.", name);

            if (Util.ThereIs(members.Find(p => !Util.ThereIs(p))))
            {
                Logger.Error(blipName + ": There is a member who doesn't exist. Abort.", name);
                Restore(true);

                return false;
            }

            foreach (Ped p in members)
            {
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

                    case "FIB":
                        {
                            p.Weapons.Give(WeaponHash.CarbineRifle, 300, false, true);
                            p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                            p.ShootRate = 900;
                            p.Armor = 50;

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
                            if (Util.GetRandomIntBelow(3) == 1)
                            {
                                Shield s = new Shield(p);

                                if (s.IsCreatedIn(p.Position.Around(5.0f)))
                                {
                                    DispatchManager.Add(s, DispatchManager.DispatchType.Shield);
                                    p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                                }
                                else s.Restore(true);
                            }

                            if (!p.Weapons.HasWeapon(WeaponHash.Pistol))
                            {
                                p.Weapons.Give(WeaponHash.Pistol, 100, false, true);
                                p.Weapons.Give(WeaponHash.SMG, 300, true, true);
                            }

                            p.ShootRate = 700;
                            p.Armor = 70;

                            break;
                        }
                }

                p.Weapons.Current.InfiniteAmmo = true;
                p.CanSwitchWeapons = true;
                AddVarietyTo(p);

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 17, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 52, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                Function.Call(Hash.SET_DRIVER_ABILITY, p, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, p, 1.0f);

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.FiringPattern = FiringPattern.BurstFireDriveby;

                p.RelationshipGroup = relationship;
                Logger.Write(false, blipName + ": Characteristics are set.", name);
            }

            spawnedVehicle.EngineRunning = true;
            Logger.Write(false, blipName + ": Ready to dispatch.", name);

            return true;
        }

        protected override BlipSprite CurrentBlipSprite { get { return BlipSprite.PoliceOfficer; } }
    }
}