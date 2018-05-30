using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class ReplacedVehicle : EntitySet
    {
        private string name;

        public ReplacedVehicle(string name) : base() { this.name = name; }

        public bool IsCreatedIn(float radius)
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

            if (nearbyVehicles.Length < 1) return false;

            for (int trycount = 0; trycount < 5; trycount++)
            {
                Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomIntBelow(nearbyVehicles.Length)];

                if (Util.WeCanReplace(selectedVehicle) && !selectedVehicle.IsPersistent && !selectedVehicle.IsAttached() && !Util.ThereIs(selectedVehicle.GetEntityAttachedTo()) && Util.SomethingIsBetweenPlayerAnd(selectedVehicle))
                {
                    Vector3 selectedPosition = selectedVehicle.Position;
                    float selectedHeading = selectedVehicle.Heading;
                    float selectedSpeed = selectedVehicle.Speed;
                    bool selectedEngineRunning = selectedVehicle.EngineRunning;
                    string selectedBlipName;
                    BlipColor selectedBlipColor;

                    selectedVehicle.Delete();
                    spawnedVehicle = Util.Create(name, selectedPosition, selectedHeading, true);

                    if (!Util.ThereIs(spawnedVehicle)) return false;
                    if (selectedEngineRunning)
                    {
                        spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                        if (Util.ThereIs(spawnedPed))
                        {
                            Script.Wait(50);
                            spawnedVehicle.EngineRunning = true;
                            spawnedVehicle.Speed = selectedSpeed;
                            spawnedPed.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "CIV" + spawnedPed.Gender.ToString().ToUpper());
                            spawnedPed.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                            Util.NaturallyRemove(spawnedPed);
                        }
                    }

                    if (Util.GetRandomIntBelow(3) == 1)
                    {
                        selectedBlipName = "Tuned ";
                        selectedBlipColor = BlipColor.Blue;
                        Util.Tune(spawnedVehicle, Util.GetRandomIntBelow(2) == 1, Util.GetRandomIntBelow(2) == 1);
                    }
                    else
                    {
                        selectedBlipName = "";
                        selectedBlipColor = BlipColor.White;
                    }

                    selectedBlipName += spawnedVehicle.FriendlyName == "NULL" ? spawnedVehicle.DisplayName.ToUpper() : spawnedVehicle.FriendlyName;

                    if (!Util.BlipIsOn(spawnedVehicle))
                    {
                        if (!Main.NoBlipOnCriminal) Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.PersonalVehicleCar, selectedBlipColor, selectedBlipName);

                        return true;
                    }
                    else Restore(true);
                }
            }

            return false;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedVehicle) || !Util.WeCanEnter(spawnedVehicle) || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
            {
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