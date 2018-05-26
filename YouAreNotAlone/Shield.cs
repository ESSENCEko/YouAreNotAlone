using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Shield : AdvancedEntity
    {
        private List<string> shieldModels;
        private Ped owner;
        private Prop shield;
        private bool attached;
        private int boneIndex;
        private Vector3 position;
        private Vector3 rotation;

        public Shield(Ped p)
        {
            this.shieldModels = new List<string> { "prop_ballistic_shield", "prop_riot_shield" };
            this.owner = p;
            this.attached = false;
            this.boneIndex = Function.Call<int>(Hash.GET_PED_BONE_INDEX, owner, 61163);
            this.position = new Vector3(0.21f, -0.11f, -0.038f);
            this.rotation = new Vector3(60.0f, 170.0f, 10.0f);
        }

        public bool IsCreatedIn(Vector3 position)
        {
            if (position.Equals(Vector3.Zero)) return false;

            Model m = shieldModels[Util.GetRandomInt(shieldModels.Count)];
            shield = World.CreateProp(m, position, true, true);
            m.MarkAsNoLongerNeeded();

            if (!Util.ThereIs(shield)) return false;

            shield.IsPersistent = true;
            shield.IsFireProof = true;
            shield.IsMeleeProof = true;
            shield.IsInvincible = true;
            shield.IsVisible = false;

            Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, owner, Function.Call<int>(Hash.GET_HASH_KEY, "Gang1H"));
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, owner, 0, false);
            owner.CanPlayGestures = false;
            attached = false;

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                if (Util.ThereIs(shield)) shield.Delete();
            }
            else
            {
                if (Util.ThereIs(shield)) shield.MarkAsNoLongerNeeded();
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(shield) || !Util.ThereIs(owner) || !shield.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            return false;
        }

        public void CheckShieldable()
        {
            if (owner.IsInVehicle()) Detach(false);
            else if (owner.IsDead) Detach(true);
            else Attach();
        }

        private void Attach()
        {
            if (!attached)
            {
                shield.AttachTo(owner, boneIndex, position, rotation);
                shield.IsVisible = true;
                attached = true;
            }
        }

        private void Detach(bool shouldBeVisible)
        {
            if (attached)
            {
                shield.Detach();
                shield.IsVisible = shouldBeVisible;
                attached = false;
            }
        }
    }
}