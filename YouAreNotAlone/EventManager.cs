﻿using GTA;
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
                foreach (ReplacedVehicle rv in replacedList)
                {
                    if (rv.CanBeNaturallyRemoved())
                    {
                        rv.Restore(true);
                        replacedList.Remove(rv);
                        break;
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
                        aggressiveList.Add(en);
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

                case EventType.Fire:
                    {
                        onFireList.Add(en);
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
                        if (replacedList.Count > 4)
                        {
                            replacedList[0].Restore(false);
                            replacedList.RemoveAt(0);
                        }

                        replacedList.Add(en);
                        break;
                    }

                case EventType.Terrorist:
                    {
                        terroristList.Add(en);
                        break;
                    }
            }
        }

        public EventManager()
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

            foreach (AggressiveDriver ad in aggressiveList) ad.CheckNitroable();
            foreach (Racers r in racerList) r.CheckNitroable();
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