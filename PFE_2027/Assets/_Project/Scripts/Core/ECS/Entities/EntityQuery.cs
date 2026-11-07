namespace PFE.Core.Scripts.ECS
{
    public struct EntityQuery
    {
        private ComponentMask all;
        private ComponentMask any;
        private ComponentMask none;

        public readonly EntityQuery WithAll<T>() where T : struct, IEntityComponent
        {
            var copy = this;
            copy.all.Set(ComponentID<T>.Value);
            return copy;
        }
        
        public readonly EntityQuery WithAny<T>() where T : struct, IEntityComponent
        {
            var copy = this;
            copy.any.Set(ComponentID<T>.Value);
            return copy;
        }
        
        public readonly EntityQuery Without<T>() where T : struct, IEntityComponent
        {
            var copy = this;
            copy.none.Set(ComponentID<T>.Value);
            return copy;
        }

        public readonly bool Matches(in ComponentMask mask) 
            => mask.MatchesAll(all) && (mask.MatchesAny(any) || any.IsEmpty) && !mask.MatchesAny(none);
    }
}