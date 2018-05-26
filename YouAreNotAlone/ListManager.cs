using GTA;
using System;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class ListManager : Script
    {
        private static List<AdvancedEntity> aggressiveList;
        private static List<AdvancedEntity> carjackerList;
        private static List<AdvancedEntity> dispatchList;
        private static List<AdvancedEntity> drivebyList;
        private static List<AdvancedEntity> gangList;
        private static List<AdvancedEntity> massacreList;
        private static List<AdvancedEntity> racerList;
        private static List<AdvancedEntity> replacedList;
        private static List<AdvancedEntity> shieldList;
        private static List<AdvancedEntity> stingerList;
        private static List<AdvancedEntity> terroristList;
        private int timeChecker;

        public static bool ReplaceSlotIsAvailable { get { return replacedList.Count < 5; } }

        public enum EventType
        {
            AggressiveDriver,
            Army,
            Carjacker,
            Cop,
            Driveby,
            GangTeam,
            Fire,
            Massacre,
            Racer,
            ReplacedVehicle,
            RoadBlock,
            Shield,
            Terrorist
        }

        static ListManager()
        {
            aggressiveList = new List<AdvancedEntity>();
            carjackerList = new List<AdvancedEntity>();
            dispatchList = new List<AdvancedEntity>();
            drivebyList = new List<AdvancedEntity>();
            gangList = new List<AdvancedEntity>();
            massacreList = new List<AdvancedEntity>();
            racerList = new List<AdvancedEntity>();
            replacedList = new List<AdvancedEntity>();
            shieldList = new List<AdvancedEntity>();
            stingerList = new List<AdvancedEntity>();
            terroristList = new List<AdvancedEntity>();
        }

        public static void Add(AdvancedEntity en, EventType type)
        {
            switch (type)
            {
                case EventType.AggressiveDriver:
                    {
                        aggressiveList.Add(en);
                        break;
                    }

                case EventType.Army:
                case EventType.Cop:
                case EventType.Fire:
                    {
                        dispatchList.Add(en);
                        break;
                    }

                case EventType.Carjacker:
                    {
                        carjackerList.Add(en);
                        break;
                    }

                case EventType.Driveby:
                    {
                        drivebyList.Add(en);
                        break;
                    }

                case EventType.GangTeam:
                    {
                        gangList.Add(en);
                        break;
                    }

                case EventType.Massacre:
                    {
                        massacreList.Add(en);
                        break;
                    }

                case EventType.Racer:
                    {
                        racerList.Add(en);
                        break;
                    }

                case EventType.ReplacedVehicle:
                    {
                        replacedList.Add(en);
                        break;
                    }

                case EventType.RoadBlock:
                    {
                        stingerList.Add(en);
                        break;
                    }

                case EventType.Shield:
                    {
                        shieldList.Add(en);
                        break;
                    }

                case EventType.Terrorist:
                    {
                        terroristList.Add(en);
                        break;
                    }
            }
        }

        public ListManager()
        {
            timeChecker = 0;
            Tick += OnTick;
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (timeChecker == 100)
            {
                CleanUp(aggressiveList);
                CleanUp(carjackerList);
                CleanUp(dispatchList);
                CleanUp(drivebyList);
                CleanUp(gangList);
                CleanUp(massacreList);
                CleanUp(racerList);
                CleanUp(replacedList);
                CleanUp(shieldList);
                CleanUp(stingerList);
                CleanUp(terroristList);

                timeChecker = 0;
            }
            else timeChecker++;

            foreach (AggressiveDriver ad in aggressiveList) ad.CheckNitroable();
            foreach (Racers r in racerList) r.CheckNitroable();
            foreach (Shield s in shieldList) s.CheckShieldable();
            foreach (Stinger s in stingerList) s.CheckStingable();
        }

        private void CleanUp(List<AdvancedEntity> l)
        {
            for (int i = l.Count - 1; i >= 0; i--)
            {
                if (l[i].ShouldBeRemoved()) l.RemoveAt(i);
            }
        }
    }
}