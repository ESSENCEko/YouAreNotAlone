using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public static class Util
    {
        private static Array vehicleColors = Enum.GetValues(typeof(VehicleColor));
        private static Array mods = Enum.GetValues(typeof(VehicleMod));
        private static Array neonColors = Enum.GetValues(typeof(KnownColor));
        private static Array neonLights = Enum.GetValues(typeof(VehicleNeonLight));
        private static Array tints = Enum.GetValues(typeof(VehicleWindowTint));

        private static Random dice = new Random();
        private static int[] wheelTypes = { 0, 1, 2, 3, 4, 5, 7, 8, 9 };
        private static int[] wheelColors = { 156, 0, 1, 11, 2, 8, 122, 27, 30, 45, 35, 33, 136, 135, 36, 41, 138, 37, 99, 90, 95, 115, 109, 153, 154, 88, 89, 91, 55, 125, 53, 56, 151, 82, 64, 87, 70, 140, 81, 145, 142, 134 };

        private static List<int> oldRelationships = new List<int>
        {
            Function.Call<int>(Hash.GET_HASH_KEY, "CIVMALE"),
            Function.Call<int>(Hash.GET_HASH_KEY, "CIVFEMALE"),
            Function.Call<int>(Hash.GET_HASH_KEY, "SECURITY_GUARD"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_LOST"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_MEXICAN"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_FAMILY"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_BALLAS"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_MARABUNTE"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_CULT"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_SALVA"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_WEICHENG"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_HILLBILLY"),
            Function.Call<int>(Hash.GET_HASH_KEY, "GANG_1"),
            Function.Call<int>(Hash.GET_HASH_KEY, "GANG_2"),
            Function.Call<int>(Hash.GET_HASH_KEY, "GANG_9"),
            Function.Call<int>(Hash.GET_HASH_KEY, "GANG_10"),
            Function.Call<int>(Hash.GET_HASH_KEY, "DEALER"),
            Function.Call<int>(Hash.GET_HASH_KEY, "PRIVATE_SECURITY"),
            Function.Call<int>(Hash.GET_HASH_KEY, "ARMY"),
            Function.Call<int>(Hash.GET_HASH_KEY, "PRISONER"),
            Function.Call<int>(Hash.GET_HASH_KEY, "FIREMAN"),
            Function.Call<int>(Hash.GET_HASH_KEY, "MEDIC")
        };
        private static List<int> newRelationships = new List<int>();
        private static int copID = Function.Call<int>(Hash.GET_HASH_KEY, "COP");
        private static int playerID = Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER");
        private static int count = 0;

        public static int GetRandomInt(int maxValue)
        {
            return dice.Next(maxValue);
        }

        public static bool ThereIs(Entity en)
        {
            return (en != null && en.Exists());
        }

        public static bool BlipIsOn(Entity en)
        {
            return (en.CurrentBlip != null && en.CurrentBlip.Exists());
        }

        public static bool SomethingIsBetween(Entity en)
        {
            if (!ThereIs(en) || en.IsInRangeOf(Game.Player.Character.Position, 50.0f)) return false;
            else
            {
                RaycastResult r = World.Raycast(GameplayCamera.Position, en.Position, IntersectOptions.Map);

                return !en.IsOnScreen || (r.DitHitAnything && r.HitCoords.DistanceTo(GameplayCamera.Position) > 30.0f);
            }
        }

        public static Vector3 GetSafePositionIn(float radius)
        {
            Entity[] nearbyEntities = World.GetNearbyEntities(Game.Player.Character.Position, radius);

            if (nearbyEntities.Length > 0)
            {
                foreach (Entity en in nearbyEntities)
                {
                    if (ThereIs(en) && !en.IsPersistent && SomethingIsBetween(en)) return en.Position;
                }
            }

            return Vector3.Zero;
        }

        public static Vector3 GetSafePositionNear(Entity entity)
        {
            Entity[] nearbyEntities = World.GetNearbyEntities(entity.Position, 100.0f);

            if (nearbyEntities.Length > 0)
            {
                foreach (Entity en in nearbyEntities)
                {
                    if (ThereIs(en) && !en.IsPersistent && SomethingIsBetween(en)) return en.Position;
                }
            }

            return Vector3.Zero;
        }

        public static bool WeCanReplace(Vehicle v)
        {
            if (ThereIs(v) && (v.Model.IsCar || v.Model.IsBike || v.Model.IsQuadbike) && v.IsDriveable) return !Game.Player.Character.IsInVehicle(v);

            return false;
        }

        public static void AddBlipOn(Entity en, float scale, BlipSprite bs, BlipColor bc, string bn)
        {
            if (ThereIs(en))
            {
                en.AddBlip();
                en.CurrentBlip.Scale = scale;
                en.CurrentBlip.Sprite = bs;
                en.CurrentBlip.Color = bc;
                en.CurrentBlip.Name = bn;
                en.CurrentBlip.IsShortRange = true;
            }
        }

        public static Ped Create(Model m, Vector3 v3)
        {
            if (m.IsValid && !v3.Equals(Vector3.Zero))
            {
                Ped p = World.CreatePed(m, v3);
                Script.Wait(50);
                m.MarkAsNoLongerNeeded();

                if (ThereIs(p)) return p;
            }

            return null;
        }

        public static Vehicle Create(Model m, Vector3 v3, float h, bool colorNeeded)
        {
            if (m.IsValid && !v3.Equals(Vector3.Zero))
            {
                Vehicle v = World.CreateVehicle(m, v3, h);
                Script.Wait(100);
                m.MarkAsNoLongerNeeded();

                if (ThereIs(v))
                {
                    if (colorNeeded)
                    {
                        v.PrimaryColor = (VehicleColor)vehicleColors.GetValue(dice.Next(vehicleColors.Length));
                        v.SecondaryColor = (VehicleColor)vehicleColors.GetValue(dice.Next(vehicleColors.Length));
                    }

                    return v;
                }
            }

            return null;
        }

        public static void Tune(Vehicle v, bool neonsAreNeeded, bool wheelsAreNeeded)
        {
            if (ThereIs(v))
            {
                v.InstallModKit();
                v.ToggleMod(VehicleToggleMod.Turbo, true);
                v.CanTiresBurst = GetRandomInt(2) == 1;
                v.WindowTint = (VehicleWindowTint)tints.GetValue(dice.Next(tints.Length));

                foreach (VehicleMod m in mods)
                {
                    if (m != VehicleMod.Horns && m != VehicleMod.FrontWheels && m != VehicleMod.BackWheels && v.GetModCount(m) > 0) v.SetMod(m, dice.Next(-1, v.GetModCount(m)), false);
                }

                if (wheelsAreNeeded)
                {
                    if (v.Model.IsCar)
                    {
                        v.WheelType = (VehicleWheelType)wheelTypes[dice.Next(wheelTypes.Length)];
                        v.SetMod(VehicleMod.FrontWheels, dice.Next(-1, v.GetModCount(VehicleMod.FrontWheels)), false);
                    }
                    else if (v.Model.IsBike || v.Model.IsQuadbike)
                    {
                        v.WheelType = VehicleWheelType.BikeWheels;
                        int modIndex = dice.Next(-1, v.GetModCount(VehicleMod.FrontWheels));

                        v.SetMod(VehicleMod.FrontWheels, modIndex, false);
                        v.SetMod(VehicleMod.BackWheels, modIndex, false);
                    }

                    v.RimColor = (VehicleColor)wheelColors[dice.Next(wheelColors.Length)];
                }

                if (neonsAreNeeded)
                {
                    v.NeonLightsColor = Color.FromKnownColor((KnownColor)neonColors.GetValue(dice.Next(neonColors.Length)));

                    foreach (VehicleNeonLight n in neonLights) v.SetNeonLightsOn(n, true);
                }
            }
        }

        public static int NewRelationship(YouAreNotAlone.CrimeType type)
        {
            int newRel = World.AddRelationshipGroup((count++).ToString());

            newRelationships.Add(newRel);
            World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, copID);

            switch (type)
            {
                case YouAreNotAlone.CrimeType.AggressiveDriver:
                case YouAreNotAlone.CrimeType.Carjacker:
                case YouAreNotAlone.CrimeType.Racer:
                    {
                        foreach (int i in newRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);

                        break;
                    }

                case YouAreNotAlone.CrimeType.Driveby:
                case YouAreNotAlone.CrimeType.Massacre:
                case YouAreNotAlone.CrimeType.Terrorist:
                    {
                        foreach (int i in oldRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);
                        foreach (int i in newRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);

                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, playerID);
                        break;
                    }

                case YouAreNotAlone.CrimeType.GangTeam:
                    {
                        foreach (int i in newRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);

                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, playerID);
                        break;
                    }
            }

            return newRel;
        }

        public static void CleanUpRelationship(int relationship)
        {
            World.RemoveRelationshipGroup(relationship);

            if (newRelationships.Contains(relationship)) newRelationships.Remove(relationship);
        }

        public static bool AnyEmergencyIsNear(Vector3 position, YouAreNotAlone.EmergencyType type)
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(position, 100.0f);

            if (nearbyPeds.Length < 1) return false;

            int id = 0;

            switch (type)
            {
                case YouAreNotAlone.EmergencyType.Army:
                    id = Function.Call<int>(Hash.GET_HASH_KEY, "ARMY");
                    break;

                case YouAreNotAlone.EmergencyType.Cop:
                    id = Function.Call<int>(Hash.GET_HASH_KEY, "COP");
                    break;

                case YouAreNotAlone.EmergencyType.Firefighter:
                    id = Function.Call<int>(Hash.GET_HASH_KEY, "FIREMAN");
                    break;

                case YouAreNotAlone.EmergencyType.Paramedic:
                    id = Function.Call<int>(Hash.GET_HASH_KEY, "MEDIC");
                    break;
            }

            foreach (Ped p in nearbyPeds)
            {
                if (p.RelationshipGroup == id && !p.Equals(Game.Player.Character) && !p.IsDead) return true;
            }

            return false;
        }
    }
}