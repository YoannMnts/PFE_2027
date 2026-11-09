using System;

namespace PFE.Core.Scripts.ECS
{
    public readonly struct QueryResult
    {
        private readonly World world;
        private readonly EntityQuery query;
        private readonly bool onlyActive;

        public QueryResult(World world, EntityQuery query, bool onlyActive = false)
        {
            this.world = world;
            this.query = query;
            this.onlyActive = onlyActive;
        }
        
        public Enumerator GetEnumerator()
        {
            return new Enumerator(world, query, onlyActive);
        }
        
        public ref struct Enumerator
        {
            private readonly World world;
            private readonly EntityQuery entityQuery;
            private readonly ReadOnlySpan<int> entities;
            private readonly ReadOnlySpan<EntityMeta> metas;
            private readonly bool onlyActive;
            
            private int index;

            public Enumerator(World world, EntityQuery entityQuery, bool onlyActive)
            {
                this.world = world;
                this.entityQuery = entityQuery;
                this.onlyActive = onlyActive;
                entities = world.Entities;
                metas = world.Metas;
                index = -1;
            }

            public bool MoveNext()
            {
                while (++index < entities.Length)
                {
                    var entityMeta = metas[index];
                    
                    if(onlyActive && entityMeta.IsActive == false)
                        continue;
                    
                    if(!entityQuery.Matches(entityMeta.Mask))
                        continue;
                    
                    return true;
                }
                
                return false;
            }
            
            public Entity Current => new Entity(entities[index], world.GetGeneration(entities[index]));
        }
    }

}