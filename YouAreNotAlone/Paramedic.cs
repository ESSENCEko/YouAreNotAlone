using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Paramedic : EmergencyFire
    {
        private List<Entity> checkedPeds;

        public Paramedic(string name, Entity target) : base(name, target) { this.checkedPeds = new List<Entity>(); }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models) { return IsCreatedIn(safePosition, models, "MEDIC"); }

        protected override void SetPedsOnDuty()
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(spawnedVehicle.Position, 100.0f);

            if (nearbyPeds.Length < 1) return;

            foreach (Ped selectedPed in nearbyPeds)
            {
                if (Util.ThereIs(selectedPed) && (selectedPed.IsDead || selectedPed.IsInjured))
                {
                    if (!checkedPeds.Contains(selectedPed))
                    {
                        target = selectedPed;
                        break;
                    }
                    else target = null;
                }
            }

            if (Util.ThereIs(target) && target.Model.IsPed)
            {
                foreach (Ped p in members)
                {
                    if (p.TaskSequenceProgress < 0)
                    {
                        TaskSequence ts = new TaskSequence();
                        ts.AddTask.RunTo(target.Position.Around(1.0f));
                        ts.AddTask.LookAt(target.Position, 1000);
                        ts.AddTask.PlayAnimation("amb@medic@standing@kneel@enter", "enter");
                        ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_c");
                        ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@exit", "exit");
                        ts.AddTask.PlayAnimation("amb@medic@standing@timeofdeath@exit", "exit");
                        ts.AddTask.Wait(1000);
                        ts.Close();

                        p.Task.PerformSequence(ts);
                        ts.Dispose();
                    }
                    else if (p.TaskSequenceProgress == 6 && !checkedPeds.Contains(target)) checkedPeds.Add(target);
                }
            }
        }
    }
}