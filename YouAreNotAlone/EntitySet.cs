using GTA;

namespace YouAreNotAlone
{
    public abstract class EntitySet
    {
        protected Ped spawnedPed;
        protected Vehicle spawnedVehicle;

        public EntitySet() { }
        public abstract void Restore();
        public abstract bool ShouldBeRemoved();
    }
}