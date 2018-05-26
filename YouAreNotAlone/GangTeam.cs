using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class GangTeam : Criminal
    {
        private List<Ped> members;
        private List<WeaponHash> closeWeapons;
        private List<WeaponHash> standoffWeapons;
        private TaskSequence ts;

        public GangTeam() : base(ListManager.EventType.GangTeam)
        {
            this.members = new List<Ped>();
            this.closeWeapons = new List<WeaponHash> { WeaponHash.Bat, WeaponHash.Hatchet, WeaponHash.Hammer, WeaponHash.Knife, WeaponHash.KnuckleDuster, WeaponHash.Machete, WeaponHash.Wrench, WeaponHash.BattleAxe, WeaponHash.Unarmed };
            this.standoffWeapons = new List<WeaponHash> { WeaponHash.MachinePistol, WeaponHash.SawnOffShotgun, WeaponHash.Pistol, WeaponHash.APPistol, WeaponHash.PumpShotgun, WeaponHash.Revolver };
            Util.CleanUpRelationship(this.relationship, ListManager.EventType.GangTeam);

            ts = new TaskSequence();
            ts.AddTask.FightAgainstHatedTargets(200.0f);
            ts.AddTask.WanderAround();
            ts.Close();
        }

        public bool IsCreatedIn(float radius, Vector3 position, List<string> selectedModels, int teamID, BlipColor teamColor, string teamName)
        {
            if (selectedModels == null) return false;

            this.relationship = teamID;

            for (int i = 0; i < 6; i++)
            {
                Ped p = Util.Create(selectedModels[Util.GetRandomInt(selectedModels.Count)], position);

                if (!Util.ThereIs(p)) continue;

                if (Util.GetRandomInt(3) == 0) p.Weapons.Give(closeWeapons[Util.GetRandomInt(closeWeapons.Count)], 1, true, true);
                else p.Weapons.Give(standoffWeapons[Util.GetRandomInt(standoffWeapons.Count)], 300, true, true);

                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                p.RelationshipGroup = relationship;
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.Armor = Util.GetRandomInt(100);

                if (!Util.BlipIsOn(p))
                {
                    Util.AddBlipOn(p, 0.7f, BlipSprite.Rampage, teamColor, teamName);
                    members.Add(p);
                }
                else p.Delete();
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore(true);
                    return false;
                }
            }

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.Delete();
                }
            }
            else
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p))
                    {
                        p.MarkAsNoLongerNeeded();

                        if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();
                    }
                }
            }

            if (relationship != 0) Util.CleanUpRelationship(relationship, ListManager.EventType.GangTeam);

            members.Clear();
        }

        public void PerformTask()
        {
            foreach (Ped p in members) p.Task.PerformSequence(ts);
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

                if (!members[i].IsInCombat && Util.AnyEmergencyIsNear(members[i].Position, ListManager.EventType.Cop)) members[i].Task.PerformSequence(ts);
                if (!members[i].IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
                    if (members[i].IsPersistent) members[i].MarkAsNoLongerNeeded();

                    members.RemoveAt(i);
                }
            }

            if (members.Count < 1)
            {
                if (relationship != 0) Util.CleanUpRelationship(relationship, ListManager.EventType.GangTeam);

                ts.Dispose();
                return true;
            }
            if (Util.ThereIs(spawnedPed)) CheckDispatch();

            return false;
        }
    }
}