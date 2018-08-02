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
        private string blipName;

        public Driveby(string name) : base(EventManager.EventType.Driveby)
        {
            this.members = new List<Ped>();
            this.name = name;
            this.blipName = "";
            Logger.Write(true, "Driveby event selected.", this.name);
        }

        public bool IsCreatedIn(float radius, List<string> selectedModels)
        {
            if (relationship == 0) return false;

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
                    Logger.Write(false, "Driveby: Found proper road.", name);

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
            else if (spawnedVehicle.PassengerSeats < 1)
            {
                Logger.Error("Driveby: Passenger seats are needed but there isn't. Abort.", name);
                Restore(true);

                return false;
            }

            Logger.Write(false, "Driveby: Created vehicle successfully.", name);
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

            Logger.Write(false, "Driveby: Tuned vehicle and created members.", name);

            if (members.Find(p => !Util.ThereIs(p)) != null)
            {
                Logger.Error("Driveby: There is a member who doesn't exist. Abort.", name);
                Restore(true);

                return false;
            }

            foreach (Ped p in members)
            {
                Util.SetCombatAttributesOf(p);
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
                Logger.Write(false, "Driveby: Characteristics are set.", name);
            }

            if (SpawnedPedExistsIn(members))
            {
                Logger.Write(false, "Driveby: Created driveby successfully.", name);
                blipName = VehicleInfo.GetNameOf(spawnedVehicle.Model.Hash);

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
                Logger.Write(false, "Driveby: Restore instanly.", name);

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m))) p.Delete();

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(false, "Driveby: Restore naturally.", name);

                foreach (Ped p in members) Util.NaturallyRemove(p);

                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUp(relationship);

            members.Clear();
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

            Logger.Write(false, "Driveby: Alive members - " + alive.ToString(), name);

            if (!Util.ThereIs(spawnedVehicle) || !SpawnedPedExistsIn(members) || alive < 1 || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "Driveby: Driveby need to be restored.", name);

                return true;
            }

            CheckDispatch();
            CheckBlockable();

            if (!Util.WeCanEnter(spawnedVehicle))
            {
                Logger.Write(false, "Driveby: Couldn't let members enter vehicle. Time to fight on foot.", name);

                foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m)))
                {
                    if (p.IsSittingInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                    else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                }

                if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m)))
                {
                    if (Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.6f, BlipSprite.GunCar, BlipColor.White, "Driveby member");
                    }
                    else if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();
                }
            }
            else if (ReadyToGoWith(members))
            {
                if (!Util.BlipIsOn(spawnedVehicle)) Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.GunCar, BlipColor.White, "Driveby " + blipName);

                foreach (Ped p in members.FindAll(m => Util.BlipIsOn(m))) p.CurrentBlip.Remove();

                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    Logger.Write(false, "Driveby: Time to driveby.", name);

                    foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m)))
                    {
                        if (p.Equals(spawnedVehicle.Driver))
                        {
                            if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 151)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, 262692); // 4 + 32 + 512 + 262144
                        }
                        else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                    }
                }
                else
                {
                    Logger.Write(false, "Driveby: There is no driver. Re-enter everyone.", name);

                    foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                        p.Task.LeaveVehicle(spawnedVehicle, false);
                }
            }
            else
            {
                if (VehicleSeatsCanBeSeatedBy(members)) Logger.Write(false, "Driveby: Assigned seats successfully.", name);
                else
                {
                    Logger.Write(false, "Driveby: Something wrong with assigning seats. Re-enter everyone.", name);

                    foreach (Ped p in members.FindAll(m => Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                        p.Task.LeaveVehicle(spawnedVehicle, false);
                }

                if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m)))
                {
                    if (Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.6f, BlipSprite.GunCar, BlipColor.White, "Driveby member");
                    }
                    else if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();
                }
            }

            return false;
        }
    }
}