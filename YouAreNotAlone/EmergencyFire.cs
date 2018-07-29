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
            Util.CleanUp(this.relationship, DispatchManager.DispatchType.CopGround);
            this.relationship = 0;
            this.targetPosition = target.Position;
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeadingToChase(safePosition.Around(50.0f), targetPosition);

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

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Write(false, blipName + ": Couldn't create vehicle. Abort.", name);

                return false;
            }

            int max = emergencyType == "FIREMAN" ? 3 : 1;

            for (int i = -1; i < spawnedVehicle.PassengerSeats && i < max; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomIntBelow(models.Count)]));
                    Script.Wait(50);
                }
            }

            Logger.Write(false, blipName + ": Created members.", name);

            if (members.Find(p => !Util.ThereIs(p)) != null)
            {
                Logger.Error(blipName + ": There is a member who doesn't exist. Abort.", name);
                Restore(true);

                return false;
            }

            foreach (Ped p in members)
            {
                AddVarietyTo(p);
                p.Weapons.RemoveAll();
                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                if (emergencyType == "FIREMAN")
                {
                    p.Weapons.Give(WeaponHash.FireExtinguisher, 100, true, true);
                    p.Weapons.Current.InfiniteAmmo = true;
                    p.CanSwitchWeapons = true;
                    p.IsFireProof = true;
                }

                Logger.Write(false, blipName + ": Characteristics are set.", name);
            }

            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                Function.Call(Hash.SET_DRIVER_ABILITY, spawnedVehicle.Driver, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedVehicle.Driver, 1.0f);
            }

            spawnedVehicle.EngineRunning = true;
            Logger.Write(false, blipName + ": Ready to dispatch.", name);

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, blipName + ": Restore instantly.", name);

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m))) p.Delete();

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(false, blipName + ": Restore naturally.", name);

                foreach (Ped p in members) Util.NaturallyRemove(p);

                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;

                Util.NaturallyRemove(spawnedVehicle);
            }

            members.Clear();
        }

        protected override BlipSprite CurrentBlipSprite { get { return BlipSprite.Hospital; } }
        protected new abstract void SetPedsOnDuty(bool onVehicleDuty);
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

            Logger.Write(false, blipName + ": Alive members - " + alive.ToString(), name);

            if (!Util.ThereIs(spawnedVehicle) || alive < 1 || members.Count < 1)
            {
                Logger.Write(false, blipName + ": Emergency fire need to be restored.", name);

                return true;
            }

            if (TargetIsFound())
            {
                if (offDuty) offDuty = false;
                if (spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    Logger.Write(false, blipName + ": Target found. Time to be on duty.", name);
                    SetPedsOnDuty(Util.WeCanEnter(spawnedVehicle) && !spawnedVehicle.IsInRangeOf(targetPosition, 30.0f));
                    RefreshBlip(false);
                }
                else
                {
                    Logger.Write(false, blipName + ": Target found but too far from player. Time to be restored.", name);

                    return true;
                }
            }
            else
            {
                if (spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
                {
                    Logger.Write(false, blipName + ": Target not found. Time to be off duty.", name);
                    SetPedsOffDuty();
                    RefreshBlip(true);
                }
                else
                {
                    Logger.Write(false, blipName + ": Target not found and too far from player. Time to be restored.", name);

                    return true;
                }
            }

            return false;
        }
    }
}