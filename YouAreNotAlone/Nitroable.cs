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
            };
            nitroAmount = 600;
            nitroCooldown = false;
        }

        private bool CanSafelyUseNitroBetween(Vector3 v1, Vector3 v2)
        {
            return !World.Raycast(v1, v1 + v2, IntersectOptions.Everything, spawnedVehicle).DitHitAnything;
        }

        public void CheckNitroable()
        {
            if (!nitroCooldown && nitroAmount > 0 && spawnedVehicle.Speed > 20.0f && spawnedVehicle.Acceleration > 0
                && CanSafelyUseNitroBetween(spawnedVehicle.Position, spawnedVehicle.ForwardVector * 10.0f))
            {
                spawnedVehicle.EnginePowerMultiplier = 7.0f;
                spawnedVehicle.EngineTorqueMultiplier = 7.0f;

                float pitch = Function.Call<float>(Hash.GET_ENTITY_PITCH, spawnedVehicle);

                if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core"))
                {
                    foreach (string exhaust in exhausts)
                    {
                        if (spawnedVehicle.HasBone(exhaust))
                        {
                            float scale = spawnedVehicle.Speed / 50;
                            Vector3 offset = spawnedVehicle.GetBoneCoord(exhaust);
                            Vector3 exhPosition = spawnedVehicle.GetOffsetFromWorldCoords(offset);
                            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
                            Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "veh_backfire", spawnedVehicle, exhPosition.X, exhPosition.Y, exhPosition.Z, 0.0f, pitch, 0.0f, scale, false, false, false);
                        }
                    }
                }
                else Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");

                nitroAmount -= 2;
            }
            else nitroAmount++;

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