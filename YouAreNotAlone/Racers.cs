using GTA.Math;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Racers : Criminal
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
            Logger.Write("Racers event selected.", "");
        }

        public bool IsCreatedIn(float radius)
        {
            if (models == null || safePosition.Equals(Vector3.Zero) || goal.Equals(Vector3.Zero))
            {
                Logger.Error("Racers: Couldn't find models or safe position or goal. Abort.", "");

                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                Racer r = new Racer(models[Util.GetRandomIntBelow(models.Count)], goal);
                Road road = new Road(Vector3.Zero, 0.0f);

                for (int cnt = 0; cnt < 5; cnt++)
                {
                    road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                    if (!road.Position.Equals(Vector3.Zero))
                    {
                        Logger.Write("Racers: Found proper road.", "");

                        break;
                    }
                }

                if (road.Position.Equals(Vector3.Zero))
                {
                    Logger.Error("Racers: Couldn't find proper road. Abort.", "");
                    Restore(true);

                    return false;
                }
                else
                {
                    Logger.Write("Racers: Creating a racer.", "");

                    if (r.IsCreatedIn(radius, road)) racers.Add(r);
                    else r.Restore(true);
                }
            }

            foreach (Racer r in racers)
            {
                if (!r.Exists())
                {
                    Logger.Write("Racers: There is a racer who doesn't exist. Abort.", "");
                    Restore(true);

                    return false;
                }
            }

            Logger.Write("Racers: Created racers successfully.", "");

            return true;
        }

        public void CheckNitroable()
        {
            foreach (Racer r in racers) r.CheckNitroable();
        }

        public override void Restore(bool instantly)
        {
            if (instantly) Logger.Write("Racers: Restore instantly.", "");
            else Logger.Write("Racers: Restore naturally.", "");

            foreach (Racer r in racers) r.Restore(instantly);

            racers.Clear();
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = racers.Count - 1; i >= 0; i--)
            {
                if (racers[i].ShouldBeRemoved()) racers.RemoveAt(i);
            }

            if (racers.Count < 1)
            {
                Logger.Write("Racers: Every racer is gone. Time to be disposed.", "");

                return true;
            }

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