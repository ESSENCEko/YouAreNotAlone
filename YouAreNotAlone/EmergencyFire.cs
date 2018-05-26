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
            Util.CleanUpRelationship(this.relationship, ListManager.EventType.Cop);
            this.relationship = 0;
            this.targetPosition = target.Position;
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, false);

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
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;
            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                Function.Call(Hash.SET_DRIVER_ABILITY, spawnedVehicle.Driver, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedVehicle.Driver, 1.0f);
                spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, targetPosition, 10.0f, 100.0f, (int)DrivingStyle.AvoidTraffic);
            }

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.Delete();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p))
                    {
                        p.AlwaysKeepTask = false;
                        p.BlockPermanentEvents = false;
                        p.MarkAsNoLongerNeeded();
                    }
                }

                if (Util.ThereIs(spawnedVehicle))
                {
                    spawnedVehicle.MarkAsNoLongerNeeded();

                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;
                }
            }

            members.Clear();
        }

        protected new abstract void SetPedsOnDuty();
        protected new void SetPedsOffDuty()
        {
            if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.HasSiren && spawnedVehicle.SirenActive)
            {
                spawnedVehicle.SirenActive = false;

                foreach (Ped p in members) p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "CIVMALE");
            }

            if (EveryoneIsSitting())
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    foreach (Ped p in members)
                    {
                        if (Util.ThereIs(p) && p.IsPersistent)
                        {
                            if (p.Equals(spawnedVehicle.Driver) && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 151)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                            else p.Task.Wait(1000);

                            p.AlwaysKeepTask = false;
                            p.BlockPermanentEvents = false;
                            p.MarkAsNoLongerNeeded();
                        }
                    }
                }
                else
                {
                    foreach (Ped p in members) p.Task.LeaveVehicle(spawnedVehicle, false);
                }
            }
            else
            {
                if (!VehicleSeatsCanBeSeatedBy(members))
                {
                    Restore(false);
                    return;
                }
            }
        }

        private new void AddVarietyTo(Ped p)
        {
            if (emergencyType == "FIREMAN")
            {
                switch (Util.GetRandomInt(3))
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
                switch (Util.GetRandomInt(4))
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

            if (!Util.ThereIs(spawnedVehicle) || !spawnedVehicle.IsDriveable || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (targetPosition.Equals(Vector3.Zero)) SetPedsOffDuty();
            else if (spawnedVehicle.IsInRangeOf(targetPosition, 30.0f) || !EveryoneIsSitting()) SetPedsOnDuty();

            return false;
        }
    }
}