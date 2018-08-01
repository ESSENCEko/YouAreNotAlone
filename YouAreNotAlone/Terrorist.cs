using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Terrorist : Criminal
    {
        private List<Ped> members;
        private string name;
        private string blipName;

        public Terrorist(string name) : base(EventManager.EventType.Terrorist)
        {
            this.members = new List<Ped>();
            this.name = name;
            this.blipName = "";
            Logger.Write(true, "Terrorist event selected.", this.name);
        }

        public bool IsCreatedIn(float radius)
        {
            if (relationship == 0) return false;

            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero))
            {
                Logger.Error("Terrorist: Couldn't find safe position. Abort.", name);

                return false;
            }

            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                if (!road.Position.Equals(Vector3.Zero))
                {
                    Logger.Write(false, "Terrorist: Found proper road.", name);

                    break;
                }
            }

            if (road.Position.Equals(Vector3.Zero))
            {
                Logger.Error("Terrorist: Couldn't find proper road. Abort.", name);

                return false;
            }

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Error("Terrorist: Couldn't create vehicle. Abort.", name);

                return false;
            }

            Logger.Write(false, "Terrorist: Created vehicle and driver.", name);
            Script.Wait(50);
            Util.Tune(spawnedVehicle, false, false, false);

            if (name == "khanjali" && spawnedVehicle.GetMod(VehicleMod.Roof) != -1) spawnedVehicle.SetMod(VehicleMod.Roof, -1, false);

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, "g_m_m_chicold_01"));
                    Script.Wait(50);
                }
            }

            Logger.Write(false, "Terrorist: Tuned vehicle and created members.", name);

            if (members.Find(p => !Util.ThereIs(p)) != null)
            {
                Logger.Error("Terrorist: There is a member who doesn't exist. Abort.", name);
                Restore(true);

                return false;
            }

            foreach (Ped p in members)
            {
                Util.SetCombatAttributesOf(p);
                p.RelationshipGroup = relationship;
                p.IsPriorityTargetForEnemies = true;
                p.CanBeShotInVehicle = false;

                p.Weapons.Give(WeaponHash.MicroSMG, 100, true, true);
                p.Weapons.Current.InfiniteAmmo = true;
                p.FiringPattern = FiringPattern.BurstFireDriveby;

                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.Task.FightAgainstHatedTargets(400.0f);
                Logger.Write(false, "Terrorist: Characteristics are set.", name);
            }

            if (SpawnedPedExistsIn(members))
            {
                Logger.Write(false, "Terrorist: Created terrorists successfully.", name);
                blipName += VehicleInfo.GetNameOf(spawnedVehicle.Model.Hash);

                return true;
            }
            else
            {
                Logger.Error("Terrorist: Driver doesn't exist. Abort.", name);
                Restore(true);

                return false;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, "Terrorist: Restore instantly.", name);

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m))) p.Delete();

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(false, "Terrorist: Restore naturally.", name);

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
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);

                    continue;
                }

                if (Util.WeCanGiveTaskTo(members[i])) alive++;
                else
                {
                    Util.NaturallyRemove(members[i]);
                    members.RemoveAt(i);
                }
            }

            Logger.Write(false, "Terrorist: Alive members - " + alive.ToString(), name);

            if (!Util.ThereIs(spawnedVehicle) || !SpawnedPedExistsIn(members) || alive < 1 || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "Terrorist: Terrorist need to be restored.", name);

                return true;
            }

            CheckDispatch();
            CheckBlockable();

            if (ReadyToGoWith(members))
            {
                if (!Util.BlipIsOn(spawnedVehicle)) Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.Tank, BlipColor.Red, "Terrorist " + blipName);

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m) && Util.BlipIsOn(m))) p.CurrentBlip.Remove();

                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    Logger.Write(false, "Terrorist: Time to driveby.", name);

                    foreach (Ped p in members.FindAll(m => Util.ThereIs(m) && Util.WeCanGiveTaskTo(m) && !m.IsInCombat)) p.Task.FightAgainstHatedTargets(400.0f);
                }
                else
                {
                    Logger.Write(false, "Terrorist: There is no driver. Re-enter everyone.", name);

                    foreach (Ped p in members.FindAll(m => Util.ThereIs(m) && Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                        p.Task.LeaveVehicle(spawnedVehicle, false);
                }
            }
            else
            {
                if (VehicleSeatsCanBeSeatedBy(members)) Logger.Write(false, "Terrorist: Assigned seats successfully.", name);
                else
                {
                    Logger.Write(false, "Terrorist: Something wrong with assigning seats. Re-enter everyone.", name);

                    foreach (Ped p in members.FindAll(m => Util.ThereIs(m) && Util.WeCanGiveTaskTo(m) && m.IsSittingInVehicle(spawnedVehicle)))
                        p.Task.LeaveVehicle(spawnedVehicle, false);
                }

                if (Util.ThereIs(spawnedVehicle) && Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m)))
                {
                    if (Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.6f, BlipSprite.Tank, BlipColor.Red, "Terrorist member");
                    }
                    else if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();
                }
            }

            return false;
        }

        private new void CheckDispatch()
        {
            if (dispatchCooldown < 15) dispatchCooldown++;
            else if (!Util.AnyEmergencyIsNear(spawnedPed.Position, DispatchManager.DispatchType.ArmyGround, type))
            {
                if (Main.DispatchAgainst(spawnedPed, type))
                {
                    Logger.Write(false, "Dispatch against", type.ToString());
                    dispatchCooldown = 0;
                }
                else if (++dispatchTry > 5)
                {
                    Logger.Write(false, "Couldn't dispatch against", type.ToString());
                    dispatchCooldown = 0;
                    dispatchTry = 0;
                }
            }
        }
    }
}