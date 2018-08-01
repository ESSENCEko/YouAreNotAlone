namespace YouAreNotAlone
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;
        protected int dispatchTry;
        protected int blockCooldown;
        protected int blockTry;
        protected EventManager.EventType type;

        public Criminal(EventManager.EventType type) : base()
        {
            this.dispatchCooldown = 7;
            this.dispatchTry = 0;
            this.blockCooldown = 0;
            this.blockTry = 0;
            this.type = type;
            this.relationship = Util.NewRelationshipOf(type);
        }

        protected void CheckDispatch()
        {
            if (dispatchCooldown < 15) dispatchCooldown++;
            else if (!Util.AnyEmergencyIsNear(spawnedPed.Position, DispatchManager.DispatchType.CopGround, type))
            {
                if (Main.DispatchAgainst(spawnedPed, type))
                {
                    Logger.Write(false, "Dispatch against", type.ToString());
                    dispatchCooldown = 0;
                }
                else if (++dispatchTry > 5)
                {
                    Logger.Write(false, "Couldn't dispatch against", type.ToString());
                    dispatchCooldown = 0;
                    dispatchTry = 0;
                }
            }
        }

        protected void CheckBlockable()
        {
            if (blockCooldown < 15) blockCooldown++;
            else if (Main.BlockRoadAgainst(spawnedPed, type))
            {
                Logger.Write(false, "Block road against", type.ToString());
                blockCooldown = 0;
            }
            else if (++blockTry > 5)
            {
                Logger.Write(false, "Couldn't block road against", type.ToString());
                blockCooldown = 0;
                blockTry = 0;
            }
        }
    }
}