﻿using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Stinger : AdvancedEntity
    {
        private List<string> wheels;
        private Vehicle owner;
        private Prop stinger;
        private Vector3[] points;

        public Stinger(Vehicle v)
        {
            this.wheels = new List<string>
            {
                "wheel_lf",
                "wheel_rf",
                "wheel_lm1",
                "wheel_rm1",
                "wheel_lr",
                "wheel_rr"
            };
            this.owner = v;
        }

        public bool IsCreatedIn(Vector3 position)
        {
            if (position.Equals(Vector3.Zero)) return false;

            Model m = "p_ld_stinger_s";
            stinger = World.CreateProp(m, position, true, true);
            m.MarkAsNoLongerNeeded();

            if (!Util.ThereIs(stinger)) return false;

            stinger.Heading = owner.Heading;
            stinger.IsPersistent = true;
            stinger.IsFireProof = true;
            stinger.IsInvincible = true;
            stinger.FreezePosition = true;

            Vector3 dimension = stinger.Model.GetDimensions();
            points = new Vector3[4];

            points[0] = stinger.Position + stinger.RightVector * dimension.X / 2 - stinger.ForwardVector * dimension.Y / 2;
            points[1] = stinger.Position - stinger.RightVector * dimension.X / 2 - stinger.ForwardVector * dimension.Y / 2;
            points[2] = stinger.Position - stinger.RightVector * dimension.X / 2 + stinger.ForwardVector * dimension.Y / 2;
            points[3] = stinger.Position + stinger.RightVector * dimension.X / 2 + stinger.ForwardVector * dimension.Y / 2;

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                if (Util.ThereIs(stinger)) stinger.Delete();
            }
            else
            {
                if (Util.ThereIs(stinger)) stinger.MarkAsNoLongerNeeded();
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(stinger) || !Util.ThereIs(owner) || !stinger.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            return false;
        }

        public void CheckStingable()
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(stinger.Position, 10.0f);

            if (nearbyVehicles.Length < 1) return;

            foreach (Vehicle v in nearbyVehicles)
            {
                if (Util.ThereIs(v) && v.IsTouching(stinger) && v.CanTiresBurst)
                {
                    for (int i = 0; i < wheels.Count; i++)
                    {
                        if (v.HasBone(wheels[i]) && !v.IsTireBurst(i) && StingerAreaContains(v.GetBoneCoord(wheels[i]))) v.BurstTire(i);
                    }
                }
            }
        }

        private bool StingerAreaContains(Vector3 v3)
        {
            bool result = false;

            for (int i = 0, j = 3; i < 4; j = i++)
            {
                if (((points[i].Y > v3.Y) != (points[j].Y > v3.Y))
                    && (v3.X < (points[j].X - points[i].X) * (v3.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X)) result = !result;
            }

            return result;
        }
    }
}