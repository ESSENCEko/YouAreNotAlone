using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Stinger : AdvancedEntity, ICheckable
    {
        private Vehicle owner;
        private Prop stinger;
        private Vector3[] points;

        public enum Wheel
        {
            wheel_lf,
            wheel_rf,
            wheel_lm1,
            wheel_rm1,
            wheel_lr,
            wheel_rr,
            wheel_lm2 = 40483,
            wheel_rm2 = 40442
        }

        public Stinger(Vehicle owner) => this.owner = owner;

        public bool IsCreatedIn(Vector3 position)
        {
            if (position.Equals(Vector3.Zero))
            {
                Logger.Write(false, "Stinger: Couldn't find safe position. Abort.", "");

                return false;
            }

            Model m = "p_ld_stinger_s";
            stinger = World.CreateProp(m, position, true, true);
            m.MarkAsNoLongerNeeded();

            if (!Util.ThereIs(stinger))
            {
                Logger.Write(false, "Stinger: Couldn't create stinger. Abort.", "");

                return false;
            }

            stinger.Heading = owner.Heading;
            stinger.IsPersistent = true;
            stinger.IsFireProof = true;
            stinger.IsInvincible = true;
            stinger.FreezePosition = true;
            stinger.LodDistance = 200;

            Vector3 dimension = stinger.Model.GetDimensions();
            points = new Vector3[4];

            points[0] = stinger.Position + stinger.RightVector * dimension.X / 2 - stinger.ForwardVector * dimension.Y / 2;
            points[1] = stinger.Position - stinger.RightVector * dimension.X / 2 - stinger.ForwardVector * dimension.Y / 2;
            points[2] = stinger.Position - stinger.RightVector * dimension.X / 2 + stinger.ForwardVector * dimension.Y / 2;
            points[3] = stinger.Position + stinger.RightVector * dimension.X / 2 + stinger.ForwardVector * dimension.Y / 2;

            Logger.Write(false, "Stinger: Created stinger successfully.", "");

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, "Stinger: Restore instanly.", "");

                if (Util.ThereIs(stinger)) stinger.Delete();
            }
            else
            {
                Logger.Write(false, "Stinger: Restore naturally.", "");
                Util.NaturallyRemove(stinger);
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(stinger) || !Util.ThereIs(owner) || !stinger.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "Stinger: Stinger need to be restored.", "");

                return true;
            }

            return false;
        }

        public void CheckAbilityUsable()
        {
            foreach (Vehicle v in new List<Vehicle>(World.GetNearbyVehicles(stinger.Position, 20.0f)).FindAll(veh => Util.ThereIs(veh) && veh.CanTiresBurst))
            {
                foreach (Wheel w in System.Enum.GetValues(typeof(Wheel)))
                {
                    if (v.HasBone(w.ToString()) && !v.IsTireBurst((int)w) && StingerAreaContains(v.GetBoneCoord(w.ToString()))) v.BurstTire((int)w);
                }
            }
        }

        private bool StingerAreaContains(Vector3 v3)
        {
            if (v3.Equals(Vector3.Zero) || v3.Z < stinger.Position.Z) return false;

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