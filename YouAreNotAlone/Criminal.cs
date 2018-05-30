namespace YouAreNotAlone
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;
        protected int blockCooldown;
        protected EventManager.EventType type;

        public Criminal(EventManager.EventType type) : base()
        {
            this.dispatchCooldown = 7;
            this.blockCooldown = 0;
            this.type = type;
            this.relationship = Util.NewRelationshipOf(type);
        }

        protected void CheckDispatch()
        {
            if (dispatchCooldown < 15) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.AnyEmergencyIsNear(spawnedPed.Position, DispatchManager.DispatchType.Cop)) Main.DispatchAgainst(spawnedPed, type);
            }
        }

        protected void CheckBlockable()
        {
            if (blockCooldown < 15) blockCooldown++;
            else
            {
                blockCooldown = 0;
                Main.BlockRoadAgainst(spawnedPed, type);
            }
        }
    }
}