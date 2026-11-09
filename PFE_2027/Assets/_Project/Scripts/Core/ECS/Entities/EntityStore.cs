namespace PFE.Core.Scripts.ECS
{
    public class EntityStore : SparseSet<EntityMeta>
    {
        public EntityStore(int capacity) : base(capacity)
        {
        }
    }
}