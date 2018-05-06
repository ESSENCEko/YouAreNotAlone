using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Massacre : Criminal
    {
        private List<Ped> members;
        private float radius;

        public Massacre() : base(YouAreNotAlone.CrimeType.Massacre)
        {
            this.members = new List<Ped>();
            this.radius = 0.0f;
        }

        public bool IsCreatedIn(float radius, Vector3 safePosition, int teamID)
        {
            this.radius = radius;
            Vector3 position = World.GetNextPositionOnSidewalk(safePosition);
            int trycount = 0;

            if (position.Equals(Vector3.Zero)) return false;

            do
            {
                Function.Call(Hash.REQUEST_ANIM_SET, "anim_group_move_ballistic");
                Function.Call(Hash.REQUEST_ANIM_SET, "move_strafe_ballistic");
                Function.Call(Hash.REQUEST_CLIP_SET, "move_ballistic_minigun");
                Script.Wait(50);

                if (++trycount > 5) return false;
            }
            while (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "anim_group_move_ballistic")
            || !Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "move_strafe_ballistic")
            || !Function.Call<bool>(Hash.HAS_CLIP_SET_LOADED, "move_ballistic_minigun"));

            for (int i = 0; i < 4; i++)
            {
                Ped p = Util.Create("hc_gunman", position);

                if (!Util.ThereIs(p)) continue;
                if (Util.GetRandomInt(4) == 1) p.Weapons.Give(WeaponHash.RPG, 25, true, true);
                else p.Weapons.Give(WeaponHash.Minigun, 1000, true, true);

                p.Weapons.Give(WeaponHash.Pistol, 100, false, false);
                p.Weapons.Current.InfiniteAmmo = true;
                p.Health = p.MaxHealth = 2000;
                p.Armor = 100;

                Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, p, 1.0f);
                Function.Call(Hash.RESET_PED_STRAFE_CLIPSET, p);
                Function.Call(Hash.SET_PED_USING_ACTION_MODE, p, true, -1, 0);

                Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, p, "anim_group_move_ballistic", 1.0f);
                Function.Call(Hash.SET_PED_STRAFE_CLIPSET, p, "move_strafe_ballistic");
                Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, p, Function.Call<int>(Hash.GET_HASH_KEY, "Ballistic"));

                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 0, 6, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 1, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 2, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 3, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 4, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 6, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 8, 2, 1, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 9, 1, 0, 0);
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, p, 10, 9, 0, 0);
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, 5, 0, false);

                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                p.RelationshipGroup = teamID;
                p.AlwaysKeepTask = true;

                p.FiringPattern = FiringPattern.FullAuto;
                p.ShootRate = 1000;
                p.CanRagdoll = false;

                if (!Util.BlipIsOn(p))
                {
                    Util.AddBlipOn(p, 0.7f, BlipSprite.Rampage, BlipColor.White, "Massacre Squad");
                    members.Add(p);
                }
                else p.Delete();
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }
            }

            PerformTask();
            return true;
        }

        private void PerformTask()
        {
            foreach (Ped p in members) p.Task.FightAgainstHatedTargets(radius);
        }

        public override void Restore()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p))
                {
                    if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();

                    p.Delete();
                }
            }

            if (relationship != 0) Util.CleanUpRelationship(relationship);

            members.Clear();
        }

        public override bool ShouldBeRemoved()
        {
            spawnedPed = null;

            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }

                if (!members[i].IsDead) spawnedPed = members[i];
                else
                {
                    if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();

                    members[i].MarkAsNoLongerNeeded();
                    members.RemoveAt(i);
                    continue;
                }

                if (!members[i].IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
                    if (members[i].IsPersistent) members[i].MarkAsNoLongerNeeded();

                    members.RemoveAt(i);
                }
            }

            if (members.Count < 1)
            {
                if (relationship != 0) Util.CleanUpRelationship(relationship);

                return true;
            }
            if (Util.ThereIs(spawnedPed)) CheckDispatch();

            return false;
        }
    }
}