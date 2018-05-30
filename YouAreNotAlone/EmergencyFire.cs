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

                if (!road.Position.Equals(Vector3.Zero)) break;
            }

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            int max = emergencyType == "FIREMAN" ? 3 : 1;

            for (int i = -1; i < spawnedVehicle.PassengerSeats && i < max; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomIntBelow(models.Count)]));
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

            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                Function.Call(Hash.SET_DRIVER_ABILITY, spawnedVehicle.Driver, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedVehicle.Driver, 1.0f);
            }

            spawnedVehicle.EngineRunning = true;
            SetPedsOnDuty();

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

        protected new void AddEmergencyBlip(bool forVehicle)
        {
            if (forVehicle)
            {
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

        protected new abstract void SetPedsOnDuty();
        protected new void SetPedsOffDuty()
        {
            if (!Util.WeCanEnter(spawnedVehicle)) Restore(false);
            else if (ReadyToGoWith(members))
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;

                    foreach (Ped p in members)
                    {
                        if (Util.ThereIs(p) && p.IsPersistent)
                        {
                            if (Util.WeCanGiveTaskTo(p))
                            {
                                if (p.Equals(spawnedVehicle.Driver) && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 151)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                                else p.Task.Wait(1000);
                            }

                            p.AlwaysKeepTask = false;
                            p.BlockPermanentEvents = false;
                            Util.NaturallyRemove(p);
                        }
                    }

                    if (spawnedVehicle.IsPersistent) Util.NaturallyRemove(spawnedVehicle);
                }
                else
                {
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
                    Restore(false);
                    return;
                }
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
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i])) members.RemoveAt(i);
            }

            if (!Util.ThereIs(spawnedVehicle) || !Util.WeCanEnter(spawnedVehicle) || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (!TargetIsFound()) SetPedsOffDuty();
            else
            {
                if (spawnedVehicle.IsInRangeOf(targetPosition, 30.0f)) onVehicleDuty = false;
                else onVehicleDuty = true;

                SetPedsOnDuty();
            }

            return false;
        }
    }
}