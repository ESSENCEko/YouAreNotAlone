using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class ReplacedVehicle : EntitySet
    {
        private string name;

        public ReplacedVehicle(string name) : base()
        {
            this.name = name;
            Logger.Write("ReplacedVehicle event selected.", name);
        }

        public bool IsCreatedIn(float radius)
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

            if (nearbyVehicles.Length < 1)
            {
                Logger.Error("ReplacedVehicle: There is no vehicle near. Abort.", name);

                return false;
            }

            for (int trycount = 0; trycount < 5; trycount++)
            {
                Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomIntBelow(nearbyVehicles.Length)];

                if (Util.WeCanReplace(selectedVehicle) && !selectedVehicle.IsPersistent && !selectedVehicle.IsAttached() && !Util.ThereIs(selectedVehicle.GetEntityAttachedTo()) && Util.SomethingIsBetweenPlayerAnd(selectedVehicle))
                {
                    Logger.Write("ReplacedVehicle: Replaceable vehicle found.", name);
                    Vector3 selectedPosition = selectedVehicle.Position;
                    float selectedHeading = selectedVehicle.Heading;
                    float selectedSpeed = selectedVehicle.Speed;
                    bool driverIsNeeded = Util.ThereIs(selectedVehicle.Driver);
                    string blipName;
                    BlipColor blipColor;

                    selectedVehicle.Delete();
                    spawnedVehicle = Util.Create(name, selectedPosition, selectedHeading, true);

                    if (!Util.ThereIs(spawnedVehicle))
                    {
                        Logger.Error("ReplacedVehicle: Couldn't create selected vehicle. Abort.", name);

                        return false;
                    }

                    Logger.Write("ReplacedVehicle: Created vehicle.", name);

                    if (driverIsNeeded)
                    {
                        spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                        if (Util.ThereIs(spawnedPed))
                        {
                            Script.Wait(50);
                            spawnedVehicle.EngineRunning = true;
                            spawnedVehicle.Speed = selectedSpeed;
                            spawnedPed.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "CIV" + spawnedPed.Gender.ToString().ToUpper());
                            spawnedPed.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                            spawnedPed.MarkAsNoLongerNeeded();
                            Logger.Write("ReplacedVehicle: Created driver.", name);
                        }
                        else Logger.Error("ReplacedVehicle: Couldn't create driver in replacing vehicle.", name);
                    }

                    if (Util.GetRandomIntBelow(3) == 1)
                    {
                        blipName = "Tuned ";
                        blipColor = BlipColor.Blue;
                        Util.Tune(spawnedVehicle, Util.GetRandomIntBelow(2) == 1, Util.GetRandomIntBelow(2) == 1);
                        Logger.Write("ReplacedVehicle: Tune replacing vehicle.", name);
                    }
                    else
                    {
                        blipName = "";
                        blipColor = BlipColor.White;
                        Logger.Write("ReplacedVehicle: Remain stock replacing vehicle.", name);
                    }

                    blipName += spawnedVehicle.FriendlyName == "NULL" ? spawnedVehicle.DisplayName.ToUpper() : spawnedVehicle.FriendlyName;

                    if (!Util.BlipIsOn(spawnedVehicle))
                    {
                        if (!Main.NoBlipOnCriminal) Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.PersonalVehicleCar, blipColor, blipName);

                        Logger.Write("ReplacedVehicle: Create replacing vehicle successfully.", name);

                        return true;
                    }
                    else
                    {
                        Logger.Error("ReplacedVehicle: Blip is already on replacing vehicle. Abort.", name);
                        Restore(true);
                    }
                }
            }

            return false;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write("ReplacedVehicle: Restore instantly.", name);

                if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write("ReplacedVehicle: Restore naturally.", name);
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedVehicle) || !Util.WeCanEnter(spawnedVehicle) || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
            {
                Logger.Write("ReplacedVehicle: Replaced vehicle need to be restored.", name);
                Restore(false);

                return true;
            }

            return false;
        }

        public bool CanBeNaturallyRemoved()
        {
            return Util.SomethingIsBetweenPlayerAnd(spawnedVehicle);
        }
    }
}