using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Paramedic : EmergencyFire
    {
        private List<int> checkedPeds;

        public Paramedic(string name, Entity target) : base(name, target, "MEDIC") { this.checkedPeds = new List<int>(); }

        protected override void SetPedsOnDuty()
        {
            if (TargetIsFound() && target.Model.IsPed)
            {
                foreach (Ped p in members)
                {
                    if (p.TaskSequenceProgress < 0)
                    {
                        TaskSequence ts = new TaskSequence();
                        ts.AddTask.RunTo(targetPosition.Around(1.0f));
                        ts.AddTask.LookAt(targetPosition, 1000);
                        ts.AddTask.PlayAnimation("amb@medic@standing@kneel@enter", "enter");
                        ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_c");
                        ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@exit", "exit");
                        ts.AddTask.PlayAnimation("amb@medic@standing@timeofdeath@exit", "exit");
                        ts.AddTask.Wait(1000);
                        ts.Close();

                        p.Task.PerformSequence(ts);
                        ts.Dispose();
                    }
                    else if (p.TaskSequenceProgress == 6 && !checkedPeds.Contains(target.Handle)) checkedPeds.Add(target.Handle);
                }
            }
        }

        private new bool TargetIsFound()
        {
            target = null;
            targetPosition = Vector3.Zero;
            Ped[] nearbyPeds = World.GetNearbyPeds(spawnedVehicle.Position, 100.0f);

            if (nearbyPeds.Length < 1) return false;

            foreach (Ped selectedPed in nearbyPeds)
            {
                if (Util.ThereIs(selectedPed) && (selectedPed.IsDead || selectedPed.IsInjured))
                {
                    if (!checkedPeds.Contains(selectedPed.Handle))
                    {
                        target = selectedPed;
                        targetPosition = target.Position;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}