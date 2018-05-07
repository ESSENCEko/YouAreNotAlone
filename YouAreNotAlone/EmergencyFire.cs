using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public abstract class EmergencyFire : Emergency
    {
        public EmergencyFire(string name, Entity target) : base(name, target) { }

        protected bool IsCreatedIn(Vector3 safePosition, List<string> models, string emergencyType)
        {
            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, (target.Position - position).ToHeading(), false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            int max = emergencyType == "FIREMAN" ? 3 : 1;

            for (int i = -1; i < spawnedVehicle.PassengerSeats && i < max; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomInt(models.Count)]));
                    Script.Wait(50);
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }

                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                if (emergencyType.Equals("FIREMAN"))
                {
                    p.Weapons.Give(WeaponHash.FireExtinguisher, 100, true, true);
                    p.Weapons.Current.InfiniteAmmo = true;
                    p.CanSwitchWeapons = true;
                    p.IsFireProof = true;
                }
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;
            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                Function.Call(Hash.SET_DRIVER_ABILITY, spawnedVehicle.Driver, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedVehicle.Driver, 1.0f);
                spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, target.Position, 10.0f, 100.0f, (int)DrivingStyle.SometimesOvertakeTraffic);
            }

            return true;
        }

        protected abstract void SetPedsOnDuty();

        protected void SetPedsOffDuty()
        {
            if (EveryoneIsSitting())
            {
                if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;
                foreach (Ped p in members)
                {
                    if (p.IsPersistent)
                    {
                        if (p.Equals(spawnedVehicle.Driver)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                        else p.Task.Wait(1000);

                        p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "CIVMALE");
                        p.MarkAsNoLongerNeeded();
                    }
                }
            }
            else
            {
                foreach (Ped p in members)
                {
                    if (!p.IsInVehicle(spawnedVehicle))
                    {
                        for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
                        {
                            if (spawnedVehicle.IsSeatFree((VehicleSeat)i) || spawnedVehicle.GetPedOnSeat((VehicleSeat)i).IsDead)
                            {
                                if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 160))
                                {
                                    p.Task.EnterVehicle(spawnedVehicle, (VehicleSeat)i, -1, 2.0f, 1);
                                    break;
                                }
                                else if (p.IsStopped && !p.IsGettingIntoAVehicle)
                                {
                                    p.SetIntoVehicle(spawnedVehicle, (VehicleSeat)i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool EveryoneIsSitting()
        {
            foreach (Ped p in members)
            {
                if (!p.IsDead && !p.IsSittingInVehicle(spawnedVehicle)) return false;
            }

            return true;
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }

                if (members[i].IsDead)
                {
                    members[i].MarkAsNoLongerNeeded();
                    members.RemoveAt(i);
                }
            }

            if (!Util.ThereIs(spawnedVehicle) || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.MarkAsNoLongerNeeded();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                return true;
            }

            if (!Util.ThereIs(target)) SetPedsOffDuty();
            else if (spawnedVehicle.IsInRangeOf(target.Position, 30.0f) || !EveryoneIsSitting()) SetPedsOnDuty();

            return false;
        }
    }
}