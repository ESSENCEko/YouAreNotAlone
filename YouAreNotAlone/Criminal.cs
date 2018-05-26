namespace YouAreNotAlone
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;
        protected int blockCooldown;
        protected ListManager.EventType type;

        public Criminal(ListManager.EventType type) : base()
        {
            this.dispatchCooldown = 7;
            this.blockCooldown = 0;
            this.type = type;
            this.relationship = Util.NewRelationship(type);
        }

        protected void CheckDispatch()
        {
            if (dispatchCooldown < 15) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.AnyEmergencyIsNear(spawnedPed.Position, ListManager.EventType.Cop)) YouAreNotAlone.DispatchAgainst(spawnedPed, type);
            }
        }

        protected void CheckBlockable()
        {
            if (blockCooldown < 15) blockCooldown++;
            else
            {
                blockCooldown = 0;
                YouAreNotAlone.BlockRoadAgainst(spawnedPed, type);
            }
        }
    }
}