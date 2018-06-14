using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Driveby : Criminal
    {
        private List<Ped> members;
        private string name;

        public Driveby(string name) : base(EventManager.EventType.Driveby)
        {
            this.members = new List<Ped>();
            this.name = name;
            Logger.ForceWrite("Driveby event selected.", this.name);
        }

        public bool IsCreatedIn(float radius, List<string> selectedModels)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero) || selectedModels == null)
            {
                Logger.Error("Driveby: Couldn't find safe position or selected models. Abort.", name);

                return false;
            }

            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                if (!road.Position.Equals(Vector3.Zero))
                {
                    Logger.Write("Driveby: Found proper road.", name);

                    break;
                }
            }

            if (road.Position.Equals(Vector3.Zero))
            {
                Logger.Error("Driveby: Couldn't find proper road. Abort.", name);

                return false;
            }

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Error("Driveby: Couldn't create vehicle. Abort.", name);

                return false;
            }

            Logger.Write("Driveby: Created vehicle successfully.", name);
            List<WeaponHash> drivebyWeaponList = new List<WeaponHash> { WeaponHash.MicroSMG, WeaponHash.Pistol, WeaponHash.APPistol, WeaponHash.CombatPistol, WeaponHash.MachinePistol, WeaponHash.MiniSMG, WeaponHash.Revolver, WeaponHash.RevolverMk2, WeaponHash.DoubleActionRevolver };
            Util.Tune(spawnedVehicle, false, (Util.GetRandomIntBelow(3) == 1), false);

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModels[Util.GetRandomIntBelow(selectedModels.Count)]));
                    Script.Wait(50);
                }
            }

            Logger.Write("Driveby: Tuned vehicle and created members.", name);

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Logger.Error("Driveby: There is a member who doesn't exist. Abort.", name);
                    Restore(true);

                    break;
                }

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 17, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);
                Function.Call(Hash.SET_DRIVER_ABILITY, p, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, p, 1.0f);

                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.Weapons.Give(drivebyWeaponList[Util.GetRandomIntBelow(drivebyWeaponList.Count)], 100, true, true);
                p.Weapons.Current.InfiniteAmmo = true;

                p.ShootRate = 1000;
                p.RelationshipGroup = relationship;
                p.IsPriorityTargetForEnemies = true;
                p.FiringPattern = FiringPattern.BurstFireDriveby;
                Logger.Write("Driveby: Characteristics are set.", name);
            }

            if (SpawnedPedExists())
            {
                Logger.Write("Driveby: Created driveby successfully.", name);

                return true;
            }
            else
            {
                Logger.Error("Driveby: Driver doesn't exist. Abort.", name);
                Restore(true);

                return false;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write("Driveby: Restore instanly.", name);

                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.Delete();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write("Driveby: Restore naturally.", name);

                foreach (Ped p in members) Util.NaturallyRemove(p);

                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUp(relationship);

            members.Clear();
        }

        private bool SpawnedPedExists()
        {
            if (Util.ThereIs(spawnedPed) && Util.WeCanGiveTaskTo(spawnedPed)) return true;
            else
            {
                spawnedPed = null;

                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p) && Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.7f, BlipSprite.GunCar, BlipColor.White, "Driveby " + VehicleName.GetNameOf(spawnedVehicle.Model.Hash));

                        Logger.Write("Driveby: Found driver and added blip on it.", name);
                        spawnedPed = p;

                        return true;
                    }
                }

                Logger.Error("Driveby: Couldn't find driver. Need to be restored.", name);

                return false;
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

            Logger.Write("Driveby: Alive members - " + alive.ToString(), name);

            if (!Util.ThereIs(spawnedVehicle) || !SpawnedPedExists() || alive < 1 || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write("Driveby: Driveby need to be restored.", name);
                Restore(false);

                return true;
            }

            CheckDispatch();
            CheckBlockable();

            if (!Util.WeCanEnter(spawnedVehicle))
            {
                Logger.Write("Driveby: Couldn't let members enter vehicle. Time to fight on foot.", name);

                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p) && Util.WeCanGiveTaskTo(p))
                    {
                        if (p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                    }
                }
            }
            else if (ReadyToGoWith(members))
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    Logger.Write("Driveby: Time to driveby.", name);

                    foreach (Ped p in members)
                    {
                        if (Util.ThereIs(p) && Util.WeCanGiveTaskTo(p))
                        {
                            if (p.Equals(spawnedVehicle.Driver))
                            {
                                if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 151)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, 262692); // 4 + 32 + 512 + 262144
                            }
                            else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                        }
                    }
                }
                else
                {
                    Logger.Write("Driveby: There is no driver. Re-enter everyone.", name);

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
                    Logger.Write("Driveby: Something wrong with assigning seats. Re-enter everyone.", name);

                    foreach (Ped p in members)
                    {
                        if (Util.ThereIs(p) && Util.WeCanGiveTaskTo(p) && p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
                else Logger.Write("Driveby: Assigned seats successfully.", name);
            }

            return false;
        }
    }
}