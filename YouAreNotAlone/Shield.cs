using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class Shield : AdvancedEntity, ICheckable
    {
        private List<string> shieldModels;
        private Ped owner;
        private Prop shield;
        private bool attached;
        private int boneIndex;
        private Vector3 position;
        private Vector3 rotation;

        public Shield(Ped owner)
        {
            this.shieldModels = new List<string> { "prop_ballistic_shield", "prop_riot_shield" };
            this.owner = owner;
            this.attached = false;
            this.boneIndex = Function.Call<int>(Hash.GET_PED_BONE_INDEX, this.owner, 61163);
            this.position = new Vector3(0.21f, -0.11f, -0.038f);
            this.rotation = new Vector3(60.0f, 170.0f, 10.0f);
        }

        public bool IsCreatedIn(Vector3 position)
        {
            if (position.Equals(Vector3.Zero))
            {
                Logger.Error("Shield: Couldn't find safe position. Abort.", "");

                return false;
            }

            Model m = shieldModels[Util.GetRandomIntBelow(shieldModels.Count)];
            shield = World.CreateProp(m, position, true, true);
            m.MarkAsNoLongerNeeded();

            if (!Util.ThereIs(shield))
            {
                Logger.Error("Shield: Couldn't create shield. Abort.", "");

                return false;
            }

            shield.IsPersistent = true;
            shield.IsFireProof = true;
            shield.IsMeleeProof = true;
            shield.IsInvincible = true;
            shield.IsVisible = false;
            shield.LodDistance = 200;

            Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, owner, Function.Call<int>(Hash.GET_HASH_KEY, "Gang1H"));
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, owner, 0, false);
            attached = false;

            Logger.Write(false, "Shield: Created shield successfully.", "");

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, "Shield: Restore instantly.", "");

                if (Util.ThereIs(shield)) shield.Delete();
            }
            else
            {
                Logger.Write(false, "Shield: Restore naturally.", "");
                Util.NaturallyRemove(shield);
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(shield) || !Util.ThereIs(owner) || !shield.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "Shield: Shield need to be restored.", "");

                return true;
            }

            return false;
        }

        public void CheckAbilityUsable()
        {
            if (owner.IsInVehicle() || owner.IsGettingIntoAVehicle) Detach(false);
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
                Logger.Write(false, "Shield: Attached to owner.", "");
            }
        }

        private void Detach(bool shouldBeVisible)
        {
            if (attached)
            {
                shield.Detach();
                shield.IsVisible = shouldBeVisible;
                attached = false;
                Logger.Write(false, "Shield: Detached from owner.", "");
            }
        }
    }
}