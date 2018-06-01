using GTA;

namespace YouAreNotAlone
{
    public class OnFire : AdvancedEntity
    {
        public Vehicle OnFireVehicle { get; private set; }

        private int dispatchCooldown;

        public OnFire()
        {
            this.dispatchCooldown = 10;
            Logger.Write("OnFire event selected.", "");
        }

        public bool IsCreatedIn(float radius, bool instantly)
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

            if (nearbyVehicles.Length < 1)
            {
                Logger.Error("OnFire: There is no vehicle near.", "");

                return false;
            }

            for (int trycount = 0; trycount < 5; trycount++)
            {
                Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomIntBelow(nearbyVehicles.Length)];

                if (Util.WeCanReplace(selectedVehicle))
                {
                    Logger.Write("OnFire: Found a proper vehicle.", "");
                    OnFireVehicle = selectedVehicle;

                    if (Util.BlipIsOn(OnFireVehicle))
                    {
                        Logger.Write("OnFire: Already got a blip. Remove it.", "");
                        OnFireVehicle.CurrentBlip.Remove();
                        Script.Wait(100);
                    }

                    OnFireVehicle.IsPersistent = true;

                    if (instantly)
                    {
                        Logger.Write("OnFire: Time to explode selected vehicle.", "");
                        Util.AddBlipOn(OnFireVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Red, "Vehicle Explosion");
                        OnFireVehicle.Explode();
                    }
                    else
                    {
                        Logger.Write("OnFire: Time to set selected vehicle on fire.", "");
                        Util.AddBlipOn(OnFireVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Yellow, "Vehicle on Fire");
                        OnFireVehicle.EngineHealth = -900.0f;
                        OnFireVehicle.IsDriveable = false;
                    }

                    break;
                }
            }

            return true;
        }

        public override void Restore(bool instantly)
        {
            Logger.Write("OnFire: Restore naturally.", "");
            Util.NaturallyRemove(OnFireVehicle);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(OnFireVehicle) || !OnFireVehicle.IsOnFire || !OnFireVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write("OnFire: On fire vehicle need to be restored.", "");
                Restore(false);

                return true;
            }

            if (dispatchCooldown < 20) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;
                Logger.Write("OnFire: Dispatch against", "Fire");
                Main.DispatchAgainst(OnFireVehicle, EventManager.EventType.Fire);
            }

            return false;
        }
    }
}