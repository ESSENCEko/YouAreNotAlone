using GTA;
using System;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class DispatchManager : Script
    {
        private static List<AdvancedEntity> armyList;
        private static List<AdvancedEntity> armyHeliList;
        private static List<AdvancedEntity> armyRoadblockList;
        private static List<AdvancedEntity> copList;
        private static List<AdvancedEntity> copHeliList;
        private static List<AdvancedEntity> copRoadblockList;
        private static List<AdvancedEntity> emList;
        private static List<AdvancedEntity> shieldList;
        private static List<AdvancedEntity> stingerList;
        private int timeChecker;

        public enum DispatchType
        {
            Army,
            ArmyHeli,
            ArmyRoadBlock,
            Cop,
            CopHeli,
            CopRoadBlock,
            Emergency,
            Shield,
            Stinger
        }

        static DispatchManager()
        {
            armyList = new List<AdvancedEntity>();
            armyHeliList = new List<AdvancedEntity>();
            armyRoadblockList = new List<AdvancedEntity>();
            copList = new List<AdvancedEntity>();
            copHeliList = new List<AdvancedEntity>();
            copRoadblockList = new List<AdvancedEntity>();
            emList = new List<AdvancedEntity>();
            shieldList = new List<AdvancedEntity>();
            stingerList = new List<AdvancedEntity>();
        }

        public static void Add(AdvancedEntity en, DispatchType type)
        {
            switch (type)
            {
                case DispatchType.Army:
                    {
                        lock (armyList) { armyList.Add(en); }

                        break;
                    }

                case DispatchType.ArmyHeli:
                    {
                        lock (armyHeliList) { armyHeliList.Add(en); }

                        break;
                    }

                case DispatchType.ArmyRoadBlock:
                    {
                        lock (armyRoadblockList) { armyRoadblockList.Add(en); }

                        break;
                    }

                case DispatchType.Cop:
                    {
                        lock (copList) { copList.Add(en); }

                        break;
                    }

                case DispatchType.CopHeli:
                    {
                        lock (copHeliList) { copHeliList.Add(en); }

                        break;
                    }

                case DispatchType.CopRoadBlock:
                    {
                        lock (copRoadblockList) { copRoadblockList.Add(en); }

                        break;
                    }

                case DispatchType.Emergency:
                    {
                        lock (emList) { emList.Add(en); }

                        break;
                    }

                case DispatchType.Shield:
                    {
                        lock (shieldList) { shieldList.Add(en); }

                        break;
                    }

                case DispatchType.Stinger:
                    {
                        lock (stingerList) { stingerList.Add(en); }

                        break;
                    }
            }

            Logger.Write("DispatchManager: Added new entity.", type.ToString());
        }

        public DispatchManager()
        {
            timeChecker = 0;
            Tick += OnTick;

            Logger.ForceWrite("DispatchManager started.", "");
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (timeChecker == 100)
            {
                CleanUp(armyList);
                CleanUp(armyHeliList);
                CleanUp(armyRoadblockList);
                CleanUp(copList);
                CleanUp(copHeliList);
                CleanUp(copRoadblockList);
                CleanUp(emList);
                CleanUp(shieldList);
                CleanUp(stingerList);

                timeChecker = 0;
            }
            else timeChecker++;

            lock (shieldList)
            {
                foreach (Shield s in shieldList) s.CheckShieldable();
            }

            lock (stingerList)
            {
                foreach (Stinger s in stingerList) s.CheckStingable();
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