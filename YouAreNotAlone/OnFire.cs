using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class OnFire : AdvancedEntity
    {
        public Vehicle OnFireVehicle { get; private set; }

        private int dispatchCooldown;
        private int dispatchTry;
        private bool instantly;
        private Blip blip;

        public OnFire()
        {
            this.dispatchCooldown = 10;
            this.dispatchTry = 0;
            this.instantly = false;
            this.blip = null;
            Logger.Write(true, "OnFire event selected.", "");
        }

        public bool IsCreatedIn(float radius, bool instantly)
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

            if (nearbyVehicles.Length < 1)
            {
                Logger.Error("OnFire: There is no vehicle near.", "");

                return false;
            }

            this.instantly = instantly;

            for (int trycount = 0; trycount < 5; trycount++)
            {
                Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomIntBelow(nearbyVehicles.Length)];

                if (Util.WeCanReplace(selectedVehicle))
                {
                    Logger.Write(false, "OnFire: Found a proper vehicle.", "");
                    OnFireVehicle = selectedVehicle;
                    OnFireVehicle.IsPersistent = true;

                    if (this.instantly)
                    {
                        Logger.Write(false, "OnFire: Time to explode selected vehicle.", "");
                        OnFireVehicle.Explode();
                    }
                    else
                    {
                        Logger.Write(false, "OnFire: Time to set selected vehicle on fire.", "");
                        OnFireVehicle.EngineHealth = -900.0f;
                        OnFireVehicle.IsDriveable = false;
                    }

                    blip = World.CreateBlip(OnFireVehicle.Position);
                    blip.Scale = 0.7f;
                    blip.Sprite = (BlipSprite)436;
                    blip.Color = BlipColor.Red;
                    blip.Name = "On Fire";
                    blip.IsShortRange = true;

                    break;
                }
            }

            return true;
        }

        private bool AnyFireNear()
        {
            OutputArgument outPos = new OutputArgument();

            if (Function.Call<bool>(Hash.GET_CLOSEST_FIRE_POS, outPos, OnFireVehicle.Position.X, OnFireVehicle.Position.Y, OnFireVehicle.Position.Z))
            {
                Vector3 position = outPos.GetResult<Vector3>();

                if (!position.Equals(Vector3.Zero) && OnFireVehicle.IsInRangeOf(position, 200.0f))
                {
                    Logger.Write(false, "OnFire: Found fire position.", "");

                    if (!blip.Position.Equals(position)) blip.Position = position;

                    return true;
                }
            }

            Entity en = null;

            if (Util.ThereIs(en = new List<Entity>(World.GetNearbyEntities(OnFireVehicle.Position, 200.0f)).Find(e => Util.ThereIs(e) && e.IsOnFire)))
            {
                Logger.Write(false, "OnFire: Found entity on fire.", "");

                if (!blip.Position.Equals(OnFireVehicle.Position)) blip.Position = en.Position;

                return true;
            }

            Logger.Write(false, "OnFire: There is no fire near.", "");

            return false;
        }

        public override void Restore(bool instantly)
        {
            Logger.Write(false, "OnFire: Restore naturally.", "");
            blip.Remove();
            Util.NaturallyRemove(OnFireVehicle);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(OnFireVehicle) || !AnyFireNear() || !OnFireVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "OnFire: On fire vehicle need to be restored.", "");

                return true;
            }

            if (dispatchCooldown < 15) dispatchCooldown++;
            else if (Main.DispatchAgainst(OnFireVehicle, EventManager.EventType.Fire))
            {
                Logger.Write(false, "OnFire: Dispatch against", "Fire");
                dispatchCooldown = 0;
            }
            else if (++dispatchTry > 5)
            {
                Logger.Write(false, "OnFire: Couldn't dispatch", "Fire");
                dispatchCooldown = 0;
                dispatchTry = 0;
            }

            return false;
        }
    }
}