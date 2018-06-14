using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public abstract class Nitroable : Criminal
    {
        private List<string> exhausts;
        private int nitroAmount;
        private bool nitroCooldown;

        public Nitroable(EventManager.EventType type) : base(type)
        {
            this.exhausts = new List<string>();
            this.nitroAmount = 600;
            this.nitroCooldown = false;
        }

        protected void SetExhausts()
        {
            exhausts = new List<string>
            {
                "exhaust",
                "exhaust_2",
                "exhaust_3",
                "exhaust_4",
                "exhaust_5",
                "exhaust_6",
                "exhaust_7",
                "exhaust_8",
                "exhaust_9",
                "exhaust_10",
                "exhaust_11",
                "exhaust_12",
                "exhaust_13",
                "exhaust_14",
                "exhaust_15",
                "exhaust_16"
            }.FindAll(s => spawnedVehicle.HasBone(s));
        }

        private bool CanSafelyUseNitroBetween(Vector3 v1, Vector3 v2)
        {
            return !World.Raycast(v1, v1 + v2, IntersectOptions.Everything, spawnedVehicle).DitHitAnything;
        }

        public void CheckNitroable()
        {
            if (!nitroCooldown && nitroAmount > 0 && spawnedVehicle.Speed > 20.0f && spawnedVehicle.Acceleration > 0
                && CanSafelyUseNitroBetween(spawnedVehicle.Position, spawnedVehicle.ForwardVector * 15.0f))
            {
                spawnedVehicle.EnginePowerMultiplier = 10.0f;
                spawnedVehicle.EngineTorqueMultiplier = 10.0f;

                if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core"))
                {
                    foreach (string exhaust in exhausts)
                    {
                        Vector3 exhPosition = spawnedVehicle.GetOffsetFromWorldCoords(spawnedVehicle.GetBoneCoord(exhaust));
                        Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
                        Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "veh_backfire", spawnedVehicle, exhPosition.X, exhPosition.Y, exhPosition.Z, 0.0f, Function.Call<float>(Hash.GET_ENTITY_PITCH, spawnedVehicle), 0.0f, spawnedVehicle.Speed / 50, false, false, false);
                    }
                }
                else Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");

                nitroAmount -= 2;
            }
            else
            {
                spawnedVehicle.EnginePowerMultiplier = 1.0f;
                spawnedVehicle.EngineTorqueMultiplier = 1.0f;
                nitroAmount++;
            }

            if (nitroAmount > 600) nitroAmount = 600;
            if (nitroAmount > 200) nitroCooldown = false;
            else if (nitroAmount <= 0)
            {
                nitroAmount = 0;
                nitroCooldown = true;
            }
        }
    }
}