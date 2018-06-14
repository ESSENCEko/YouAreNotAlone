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
            Logger.ForceWrite(blipName + ": Time to dispatch.", this.name);
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
            spawnedVehicle.PrimaryColor = spawnedVehicle.SecondaryColor = VehicleColor.MetallicBlack;
            Function.Call(Hash.SET_HELI_BLADES_FULL_SPEED, spawnedVehicle);
            SetPedsOnDuty(true);
            Logger.Write(blipName + ": Ready to dispatch.", name);

            return true;
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

            if (!Util.ThereIs(spawnedVehicle) || alive < 1 || members.Count < 1)
            {
                Logger.Write(blipName + ": Emergency helicopter need to be restored.", name);
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
                if (offDuty) offDuty = false;
                if (!spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    Logger.Write(blipName + ": Target found but too far from player. Time to be restored.", name);
                    Restore(false);

                    return true;
                }
                else
                {
                    Logger.Write(blipName + ": Target found. Time to be on duty.", name);
                    SetPedsOnDuty(Util.WeCanEnter(spawnedVehicle) || spawnedVehicle.IsInAir);
                }
            }

            return false;
        }

        protected override BlipSprite CurrentBlipSprite { get { return (Util.WeCanEnter(spawnedVehicle) || spawnedVehicle.IsInAir) ? BlipSprite.PoliceHelicopterAnimated : BlipSprite.PoliceOfficer; } }
    }
}