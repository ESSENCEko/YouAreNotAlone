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
            else if (!Util.AnyEmergencyIsNear(spawnedPed.Position, DispatchManager.DispatchType.Cop))
            {
                Main.DispatchAgainst(spawnedPed, type);
                Logger.Write("Dispatch against", type.ToString());
                dispatchCooldown = 0;
            }
        }

        protected void CheckBlockable()
        {
            if (blockCooldown < 15) blockCooldown++;
            else if (Main.BlockRoadAgainst(spawnedPed, type))
            {
                Logger.Write("Block road against", type.ToString());
                blockCooldown = 0;
            }
        }
    }
}