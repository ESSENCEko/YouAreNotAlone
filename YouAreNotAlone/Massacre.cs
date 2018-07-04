using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Massacre : Criminal
    {
        private List<Ped> members;

        public Massacre() : base(EventManager.EventType.Massacre)
        {
            this.members = new List<Ped>();
            Logger.Write(true, "Massacre event selected.", "");
        }

        public bool IsCreatedIn(float radius, Vector3 safePosition)
        {
            if (relationship == 0) return false;

            Vector3 position = World.GetNextPositionOnSidewalk(safePosition);
            int trycount = 0;

            if (position.Equals(Vector3.Zero))
            {
                Logger.Error("Massacre: Couldn't find safe position. Abort.", "");

                return false;
            }

            while (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "anim_group_move_ballistic")
            || !Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "move_strafe_ballistic")
            || !Function.Call<bool>(Hash.HAS_CLIP_SET_LOADED, "move_ballistic_minigun"))
            {
                Function.Call(Hash.REQUEST_ANIM_SET, "anim_group_move_ballistic");
                Function.Call(Hash.REQUEST_ANIM_SET, "move_strafe_ballistic");
                Function.Call(Hash.REQUEST_CLIP_SET, "move_ballistic_minigun");
                Script.Wait(50);

                if (++trycount > 5)
                {
                    Logger.Error("Massacre: Couldn't load anim/clip sets. Abort.", "");
                    Restore(true);

                    return false;
                }
            }

            Logger.Write(false, "Massacre: Anim/clip sets are loaded. Creating members.", "");

            for (int i = 0; i < 4; i++)
            {
                Ped p = Util.Create("hc_gunman", position);

                if (!Util.ThereIs(p))
                {
                    Logger.Error("Massacre: Couldn't create a member. Skip to set characteristics.", "");

                    continue;
                }

                if (Util.GetRandomIntBelow(4) == 1) p.Weapons.Give(WeaponHash.RPG, 25, true, true);
                else p.Weapons.Give(WeaponHash.Minigun, 1000, true, true);

                p.Weapons.Give(WeaponHash.Pistol, 100, false, false);
                p.Weapons.Current.InfiniteAmmo = true;
                p.Health = p.MaxHealth = 500;
                p.Armor = 100;

                Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, p, 1.0f);
                Function.Call(Hash.RESET_PED_STRAFE_CLIPSET, p);

                Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, p, "anim_group_move_ballistic", 1.0f);
                Function.Call(Hash.SET_PED_STRAFE_CLIPSET, p, "move_strafe_ballistic");
                Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, p, Function.Call<int>(Hash.GET_HASH_KEY, "Ballistic"));

                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 0, Util.GetRandomIntBelow(7), 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 1, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 2, 0, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 3, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 4, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 6, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 8, 7, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 9, 1, 0, 0);

                switch (Util.GetRandomIntBelow(4))
                {
                    case 0:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, 5, 0, true);

                        break;

                    case 1:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 1, 0, true);

                        break;

                    case 2:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 2, 0, true);

                        break;

                    case 3:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 3, 0, true);

                        break;
                }

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 17, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                Function.Call(Hash.SET_PED_PATH_CAN_USE_CLIMBOVERS, p, false);
                Function.Call(Hash.SET_PED_PATH_CAN_USE_LADDERS, p, false);
                Function.Call(Hash.SET_PED_PATH_AVOID_FIRE, p, false);

                p.RelationshipGroup = relationship;
                p.IsPriorityTargetForEnemies = true;
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                p.FiringPattern = FiringPattern.FullAuto;
                p.ShootRate = 1000;
                p.CanRagdoll = false;
                p.CanWrithe = false;

                p.IsFireProof = true;
                p.CanSwitchWeapons = true;
                p.Task.FightAgainstHatedTargets(400.0f);
                Logger.Write(false, "Massacre: Characteristics are set.", "");

                if (Util.BlipIsOn(p))
                {
                    Logger.Error("Massacre: Blip is already on the member. Delete this member to abort.", "");
                    p.Delete();
                }
                else
                {
                    Util.AddBlipOn(p, 0.7f, BlipSprite.Rampage, BlipColor.White, "Massacre Squad");
                    Logger.Write(false, "Massacre: Create a member successfully.", "");
                    members.Add(p);
                }
            }

            if (Util.ThereIs(members.Find(p => !Util.ThereIs(p))))
            {
                Logger.Error("Massacre: There is a member who doesn't exist. Abort.", "");
                Restore(true);

                return false;
            }
            else
            {
                Logger.Write(false, "Massacre: Create massacre squad successfully.", "");

                return true;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, "Massacre: Restore instanly.", "");

                foreach (Ped p in members.FindAll(m => Util.ThereIs(m))) p.Delete();
            }
            else
            {
                Logger.Write(false, "Massacre: Restore naturally.", "");

                foreach (Ped p in members) Util.NaturallyRemove(p);
            }

            if (relationship != 0) Util.CleanUp(relationship);
            if (Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "anim_group_move_ballistic")) Function.Call(Hash.REMOVE_ANIM_SET, "anim_group_move_ballistic");
            if (Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "move_strafe_ballistic")) Function.Call(Hash.REMOVE_ANIM_SET, "move_strafe_ballistic");
            if (Function.Call<bool>(Hash.HAS_CLIP_SET_LOADED, "move_ballistic_minigun")) Function.Call(Hash.REMOVE_CLIP_SET, "move_ballistic_minigun");

            members.Clear();
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);

                    continue;
                }

                if (!Util.WeCanGiveTaskTo(members[i]) || !members[i].IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    Logger.Write(false, "Massacre: Found a member who died or out of range. Need to be removed.", "");
                    Util.NaturallyRemove(members[i]);
                    members.RemoveAt(i);

                    continue;
                }

                if (!members[i].IsInCombat) members[i].Task.FightAgainstHatedTargets(400.0f);
            }

            spawnedPed = null;

            if (members.Count > 0 && Util.ThereIs(spawnedPed = members.Find(p => Util.ThereIs(p) && Util.WeCanGiveTaskTo(p)))) CheckDispatch();
            else
            {
                Logger.Write(false, "Massacre: Everyone is gone. Time to be disposed.", "");

                return true;
            }

            return false;
        }
    }
}