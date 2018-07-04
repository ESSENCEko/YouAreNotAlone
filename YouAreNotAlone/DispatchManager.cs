using System;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class DispatchManager : AdvancedScript
    {
        private static List<AdvancedEntity> armyGroundList;
        private static List<AdvancedEntity> armyHeliList;
        private static List<AdvancedEntity> armyRoadblockList;
        private static List<AdvancedEntity> copGroundList;
        private static List<AdvancedEntity> copHeliList;
        private static List<AdvancedEntity> copRoadblockList;
        private static List<AdvancedEntity> emList;
        private static List<AdvancedEntity> shieldList;
        private static List<AdvancedEntity> stingerList;
        private int timeChecker;

        public enum DispatchType
        {
            ArmyGround,
            ArmyHeli,
            ArmyRoadBlock,
            CopGround,
            CopHeli,
            CopRoadBlock,
            Emergency,
            Shield,
            Stinger
        }

        static DispatchManager()
        {
            armyGroundList = new List<AdvancedEntity>();
            armyHeliList = new List<AdvancedEntity>();
            armyRoadblockList = new List<AdvancedEntity>();
            copGroundList = new List<AdvancedEntity>();
            copHeliList = new List<AdvancedEntity>();
            copRoadblockList = new List<AdvancedEntity>();
            emList = new List<AdvancedEntity>();
            shieldList = new List<AdvancedEntity>();
            stingerList = new List<AdvancedEntity>();
        }

        public static bool Add(AdvancedEntity en, DispatchType type)
        {
            switch (type)
            {
                case DispatchType.ArmyGround: return SafelyAddTo(armyGroundList, en, type);
                case DispatchType.ArmyHeli: return SafelyAddTo(armyHeliList, en, type);
                case DispatchType.ArmyRoadBlock: return SafelyAddTo(armyRoadblockList, en, type);
                case DispatchType.CopGround: return SafelyAddTo(copGroundList, en, type);
                case DispatchType.CopHeli: return SafelyAddTo(copHeliList, en, type);
                case DispatchType.CopRoadBlock: return SafelyAddTo(copRoadblockList, en, type);
                case DispatchType.Emergency: return SafelyAddTo(emList, en, type);
                case DispatchType.Shield: return SafelyAddTo(shieldList, en, type);
                case DispatchType.Stinger: return SafelyAddTo(stingerList, en, type);
                default: return false;
            }
        }

        public DispatchManager()
        {
            timeChecker = 0;
            Tick += OnTick;

            Logger.Write(true, "DispatchManager started.", "");
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (timeChecker == 100)
            {
                SafelyCleanUp(armyGroundList, DispatchType.ArmyGround);
                SafelyCleanUp(armyHeliList, DispatchType.ArmyHeli);
                SafelyCleanUp(armyRoadblockList, DispatchType.ArmyRoadBlock);
                SafelyCleanUp(copGroundList, DispatchType.CopGround);
                SafelyCleanUp(copHeliList, DispatchType.CopHeli);
                SafelyCleanUp(copRoadblockList, DispatchType.CopRoadBlock);
                SafelyCleanUp(emList, DispatchType.Emergency);
                SafelyCleanUp(shieldList, DispatchType.Shield);
                SafelyCleanUp(stingerList, DispatchType.Stinger);

                timeChecker = 0;
            }
            else timeChecker++;

            SafelyCheckAbilityOf(shieldList, DispatchType.Shield);
            SafelyCheckAbilityOf(stingerList, DispatchType.Stinger);
        }
    }
}