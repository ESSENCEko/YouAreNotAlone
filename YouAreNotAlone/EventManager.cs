using System;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class EventManager : AdvancedScript
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

        public static bool ReplaceSlotIsAvailable() { return replacedList.Count < 5; }

        public static bool Add(AdvancedEntity en, EventType type)
        {
            switch (type)
            {
                case EventType.AggressiveDriver: return SafelyAddTo(aggressiveList, en, type);
                case EventType.Carjacker: return SafelyAddTo(carjackerList, en, type);
                case EventType.Driveby: return SafelyAddTo(drivebyList, en, type);
                case EventType.Fire: return SafelyAddTo(onFireList, en, type);
                case EventType.GangTeam: return SafelyAddTo(gangList, en, type);
                case EventType.Massacre: return SafelyAddTo(massacreList, en, type);
                case EventType.Racer: return SafelyAddTo(racerList, en, type);
                case EventType.ReplacedVehicle: return SafelyAddTo(replacedList, en, type);
                case EventType.Terrorist: return SafelyAddTo(terroristList, en, type);
                default: return false;
            }
        }

        public EventManager()
        {
            timeChecker = 0;
            Tick += OnTick;

            Logger.Write(true, "EventManager started.", "");
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (timeChecker == 100)
            {
                SafelyCleanUp(aggressiveList, EventType.AggressiveDriver);
                SafelyCleanUp(carjackerList, EventType.Carjacker);
                SafelyCleanUp(drivebyList, EventType.Driveby);
                SafelyCleanUp(gangList, EventType.GangTeam);
                SafelyCleanUp(massacreList, EventType.Massacre);
                SafelyCleanUp(onFireList, EventType.Fire);
                SafelyCleanUp(racerList, EventType.Racer);
                SafelyCleanUp(replacedList, EventType.ReplacedVehicle);
                SafelyCleanUp(terroristList, EventType.Terrorist);

                timeChecker = 0;
            }
            else timeChecker++;

            SafelyCheckAbilityOf(aggressiveList, EventType.AggressiveDriver);
            SafelyCheckAbilityOf(racerList, EventType.Racer);
        }
    }
}