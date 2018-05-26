namespace YouAreNotAlone
{
    public abstract class AdvancedEntity
    {
        public abstract void Restore(bool instantly);
        public abstract bool ShouldBeRemoved();
    }
}