using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Firefighter : EmergencyFire
    {
        public Firefighter(string name, Entity target) : base(name, target, "FIREMAN") { }

        protected override void SetPedsOnDuty()
        {
            if (TargetIsFound())
            {
                foreach (Ped p in members)
                {
                    if (p.TaskSequenceProgress < 0)
                    {
                        TaskSequence ts = new TaskSequence();
                        ts.AddTask.RunTo(targetPosition.Around(3.0f));
                        ts.AddTask.ShootAt(targetPosition, 10000, FiringPattern.FullAuto);
                        ts.Close();

                        p.Task.PerformSequence(ts);
                        ts.Dispose();
                    }
                }
            }
        }

        private new bool TargetIsFound()
        {
            target = null;
            targetPosition = Vector3.Zero;
            OutputArgument outPos = new OutputArgument();

            if (Function.Call<bool>(Hash.GET_CLOSEST_FIRE_POS, outPos, spawnedVehicle.Position.X, spawnedVehicle.Position.Y, spawnedVehicle.Position.Z))
            {
                Vector3 position = outPos.GetResult<Vector3>();

                if (!position.Equals(Vector3.Zero) && spawnedVehicle.IsInRangeOf(position, 100.0f))
                {
                    targetPosition = position;
                    return true;
                }
            }

            Entity[] nearbyEntities = World.GetNearbyEntities(spawnedVehicle.Position, 100.0f);

            if (nearbyEntities.Length < 1) return false;

            foreach (Entity en in nearbyEntities)
            {
                if (Util.ThereIs(en) && en.IsOnFire)
                {
                    target = en;
                    targetPosition = target.Position;
                    return true;
                }
            }

            return false;
        }
    }
}