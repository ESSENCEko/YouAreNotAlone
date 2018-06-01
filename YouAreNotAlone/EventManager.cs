using GTA;
using System;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class EventManager : Script
    {
        private static List<AdvancedEntity> aggressiveList;
        private static List<AdvancedEntity> carjackerList;
        private static List<AdvancedEntity> drivebyList;
        private static List<AdvancedEntity> gangList;
        private static List<AdvancedEntity> massacreList;
        private static List<AdvancedEntity> onFireList;
        private static List<AdvancedEntity> racerList;
        private static List<AdvancedEntity> replacedList;
        private static List<AdvancedEntity> terroristList;
        private int timeChecker;

        public enum EventType
        {
            AggressiveDriver,
            Carjacker,
            Driveby,
            Fire,
            GangTeam,
            Massacre,
            Racer,
            ReplacedVehicle,
            Terrorist
        }

        static EventManager()
        {
            aggressiveList = new List<AdvancedEntity>();
            carjackerList = new List<AdvancedEntity>();
            drivebyList = new List<AdvancedEntity>();
            gangList = new List<AdvancedEntity>();
            massacreList = new List<AdvancedEntity>();
            onFireList = new List<AdvancedEntity>();
            racerList = new List<AdvancedEntity>();
            replacedList = new List<AdvancedEntity>();
            terroristList = new List<AdvancedEntity>();
        }

        public static bool ReplaceSlotIsAvailable()
        {
            if (replacedList.Count < 5) return true;
            else
            {
                Logger.Write("EventManager: Replace slot is full. Search for removable one.", "");

                lock (replacedList)
                {
                    foreach (ReplacedVehicle rv in replacedList)
                    {
                        if (rv.CanBeNaturallyRemoved())
                        {
                            Logger.Write("EventManager: Found removable one.", "");
                            rv.Restore(true);
                            replacedList.Remove(rv);

                            break;
                        }
                    }
                }

                return replacedList.Count < 5;
            }
        }

        public static void Add(AdvancedEntity en, EventType type)
        {
            switch (type)
            {
                case EventType.AggressiveDriver:
                    {
                        lock (aggressiveList) { aggressiveList.Add(en); }

                        break;
                    }

                case EventType.Carjacker:
                    {
                        lock (carjackerList) { carjackerList.Add(en); }

                        break;
                    }

                case EventType.Driveby:
                    {
                        lock (drivebyList) { drivebyList.Add(en); }

                        break;
                    }

                case EventType.Fire:
                    {
                        lock (onFireList) { onFireList.Add(en); }

                        break;
                    }

                case EventType.GangTeam:
                    {
                        lock (gangList) { gangList.Add(en); }

                        break;
                    }

                case EventType.Massacre:
                    {
                        lock (massacreList) { massacreList.Add(en); }

                        break;
                    }

                case EventType.Racer:
                    {
                        lock (racerList) { racerList.Add(en); }

                        break;
                    }

                case EventType.ReplacedVehicle:
                    {
                        lock (replacedList) { replacedList.Add(en); }

                        break;
                    }

                case EventType.Terrorist:
                    {
                        lock (terroristList) { terroristList.Add(en); }

                        break;
                    }
            }

            Logger.Write("EventManager: Added new entity.", type.ToString());
        }

        public EventManager()
        {
            timeChecker = 0;
            Tick += OnTick;

            Logger.Write("EventManager started.", "");
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (timeChecker == 100)
            {
                CleanUp(aggressiveList);
                CleanUp(carjackerList);
                CleanUp(drivebyList);
                CleanUp(gangList);
                CleanUp(massacreList);
                CleanUp(onFireList);
                CleanUp(racerList);
                CleanUp(replacedList);
                CleanUp(terroristList);

                timeChecker = 0;
            }
            else timeChecker++;

            lock (aggressiveList)
            {
                foreach (AggressiveDriver ad in aggressiveList) ad.CheckNitroable();
            }

            lock (racerList)
            {
                foreach (Racers r in racerList) r.CheckNitroable();
            }
        }

        private void CleanUp(List<AdvancedEntity> l)
        {
            lock (l)
            {
                for (int i = l.Count - 1; i >= 0; i--)
                {
                    if (l[i].ShouldBeRemoved()) l.RemoveAt(i);
                }
            }
        }
    }
}