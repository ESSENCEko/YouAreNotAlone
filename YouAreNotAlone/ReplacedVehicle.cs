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
                Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                if (Util.WeCanReplace(selectedVehicle) && !selectedVehicle.IsPersistent && !Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, selectedVehicle) && Util.SomethingIsBetween(selectedVehicle))
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

                    System.IO.File.WriteAllText(@"lastCreatedVehicle.log", "[" + System.DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss") + "] " + name);

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
                            spawnedPed.MarkAsNoLongerNeeded();
                        }
                    }

                    if (Util.GetRandomInt(3) == 1)
                    {
                        selectedBlipName = "Tuned ";
                        selectedBlipColor = BlipColor.Blue;
                        Util.Tune(spawnedVehicle, Util.GetRandomInt(2) == 1, Util.GetRandomInt(3) == 1);
                    }
                    else
                    {
                        selectedBlipName = "";
                        selectedBlipColor = BlipColor.White;
                    }

                    if (spawnedVehicle.FriendlyName == "NULL") selectedBlipName += spawnedVehicle.DisplayName.ToUpper();
                    else selectedBlipName += spawnedVehicle.FriendlyName;

                    if (!Util.BlipIsOn(spawnedVehicle))
                    {
                        Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.PersonalVehicleCar, selectedBlipColor, selectedBlipName);
                        return true;
                    }
                    else Restore(true);
                }
                else selectedVehicle = null;
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
                if (Util.ThereIs(spawnedPed)) spawnedPed.MarkAsNoLongerNeeded();
                if (Util.ThereIs(spawnedVehicle))
                {
                    spawnedVehicle.MarkAsNoLongerNeeded();

                    if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();
                }
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedVehicle) || !spawnedVehicle.IsDriveable || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
            {
                Restore(false);
                return true;
            }

            return false;
        }
    }
}