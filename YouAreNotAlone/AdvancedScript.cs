using GTA;
using System;
using System.Collections.Generic;
using System.Threading;

namespace YouAreNotAlone
{
    public abstract class AdvancedScript : Script
    {
        protected static bool SafelyAddTo(List<AdvancedEntity> list, AdvancedEntity item, string name, Enum type)
        {
            if (list == null || item == null || name == null) return false;

            bool lockTaken = false;

            try
            {
                Monitor.Enter(list, ref lockTaken);
                list.Add(item);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message + "\n" + e.StackTrace, type.ToString());
            }
            finally
            {
                if (lockTaken)
                {
                    Logger.Write(false, name + ": Successfully added new entity.", type.ToString());
                    Monitor.Exit(list);
                }
            }

            return lockTaken;
        }

        protected static void SafelyCleanUp(List<AdvancedEntity> list, Enum type)
        {
            if (list == null || list.Count < 1) return;

            bool lockTaken = false;

            try
            {
                Monitor.Enter(list, ref lockTaken);

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].ShouldBeRemoved())
                    {
                        list[i].Restore(false);
                        list.RemoveAt(i);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message + "\n" + e.StackTrace, type.ToString());
            }
            finally
            {
                if (lockTaken) Monitor.Exit(list);
            }
        }

        protected static void SafelyCheckAbilityOf(List<AdvancedEntity> list, Enum type)
        {
            if (list == null || list.Count < 1) return;

            bool lockTaken = false;

            try
            {
                Monitor.Enter(list, ref lockTaken);

                foreach (ICheckable item in list.FindAll(ae => ae is ICheckable)) item.CheckAbilityUsable();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message + "\n" + e.StackTrace, type.ToString());
            }
            finally
            {
                if (lockTaken) Monitor.Exit(list);
            }
        }
    }
}