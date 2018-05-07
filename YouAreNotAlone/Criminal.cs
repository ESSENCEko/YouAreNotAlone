namespace YouAreNotAlone
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;
        protected YouAreNotAlone.CrimeType type;

        public Criminal(YouAreNotAlone.CrimeType type) : base()
        {
            this.relationship = 0;
            this.dispatchCooldown = 0;
            this.type = type;
        }

        protected void CheckDispatch()
        {
            if (dispatchCooldown < 15) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.AnyEmergencyIsNear(spawnedPed.Position, YouAreNotAlone.EmergencyType.Cop)) YouAreNotAlone.DispatchAgainst(spawnedPed, type);
            }
        }
    }
}