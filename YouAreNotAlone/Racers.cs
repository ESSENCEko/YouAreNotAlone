using GTA.Math;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Racers : Criminal, ICheckable
    {
        private List<Racer> racers;
        private List<string> models;
        private Vector3 safePosition;
        private Vector3 goal;

        public Racers(List<string> models, Vector3 position, Vector3 goal) : base(EventManager.EventType.Racer)
        {
            this.racers = new List<Racer>();
            this.models = models;
            this.safePosition = position;
            this.goal = goal;

            Util.CleanUp(this.relationship);
            this.relationship = 0;
            Logger.Write(true, "Racers event selected.", "");
        }

        public bool IsCreatedIn(float radius)
        {
            if (models == null || goal.Equals(Vector3.Zero))
            {
                Logger.Error("Racers: Couldn't find models or goal. Abort.", "");

                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                Racer r = new Racer(models[Util.GetRandomIntBelow(models.Count)], goal);

                for (int cnt = 0; cnt < 5; cnt++)
                {
                    Road road = Util.GetNextPositionOnStreetWithHeadingToChase(safePosition.Around(50.0f), goal);

                    if (road != null)
                    {
                        Logger.Write(false, "Racers: Found proper road. Creating a racer.", "");

                        if (r.IsCreatedIn(radius, road))
                        {
                            racers.Add(r);

                            break;
                        }
                        else r.Restore(true);
                    }
                }
            }

            if (racers.Find(r => !r.Exists()) != null)
            {
                Logger.Write(false, "Racers: There is a racer who doesn't exist. Abort.", "");
                Restore(true);

                return false;
            }
            else
            {
                Logger.Write(false, "Racers: Created racers successfully.", "");

                return true;
            }
        }

        public void CheckAbilityUsable()
        {
            foreach (Racer r in racers) r.CheckNitroable();
        }

        public override void Restore(bool instantly)
        {
            if (instantly) Logger.Write(false, "Racers: Restore instantly.", "");
            else Logger.Write(false, "Racers: Restore naturally.", "");

            foreach (Racer r in racers) r.Restore(instantly);

            racers.Clear();
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = racers.Count - 1; i >= 0; i--)
            {
                if (racers[i].ShouldBeRemoved())
                {
                    racers[i].Restore(false);
                    racers.RemoveAt(i);
                }
            }

            if (racers.Count < 1)
            {
                Logger.Write(false, "Racers: Every racer is gone. Time to be disposed.", "");

                return true;
            }

            spawnedPed = null;
            float distance = float.MaxValue;

            foreach (Racer r in racers)
            {
                if (r.Exists() && r.RemainingDistance < distance)
                {
                    distance = r.RemainingDistance;
                    spawnedPed = r.Driver;
                }
            }

            if (Util.ThereIs(spawnedPed))
            {
                CheckDispatch();
                CheckBlockable();
            }

            return false;
        }
    }
}