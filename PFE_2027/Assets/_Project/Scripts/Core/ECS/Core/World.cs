using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace PFE.Core.Scripts.ECS
{
    public partial class World
    {
        public ReadOnlySpan<EntityMeta> Metas => entityStores.AllElements;
        public ReadOnlySpan<int> Entities => entityStores.AllIDs;
        
        private readonly int maxEntities;
        
        private readonly IComponentStore[] componentStores;
        private readonly EntityStore entityStores;
        private readonly int[] generations;
        
        private readonly Stack<int> freeIds = new ();
        private int nextId;
        
        public World(int maxEntities)
        {
            this.maxEntities = maxEntities;
            componentStores = new IComponentStore[ComponentRegistry.MAX_COMPONENTS];
            entityStores = new EntityStore(maxEntities);
            generations = new int[maxEntities];
        }
        
        public int GetGeneration(int entityID) => entityID >= 0 && entityID < generations.Length ? generations[entityID] : 0;

        public ComponentStore<T> GetOrCreateStore<T>() where T : IEntityComponent
        {
            var componentID = ComponentID<T>.Value;
            
            if(componentStores[componentID] is ComponentStore<T> store)
                return store;
            
            var newStore = new ComponentStore<T>(maxEntities);
            componentStores[componentID] = newStore;
            return newStore;
        }

        public bool TryGetStore<T>(out ComponentStore<T> store) where T : IEntityComponent
        {
            var componentStore = componentStores[ComponentID<T>.Value];
            
            if (componentStore == null)
            {
                store = null;
                return false;
            }

            if (componentStore is not ComponentStore<T> compatible)
            {
                store = null;
                return false;
            }
            
            store = compatible;
            return true;
        }
        
        public Entity CreateEntity()
        {
            if(freeIds.Count == 0 && nextId >= maxEntities)
                throw new InvalidOperationException($"[World] Capacity of {maxEntities} entities exceeded.");
            
            var id = freeIds.Count > 0 ? freeIds.Pop() : nextId++;
            entityStores.Set(id, new EntityMeta(true));
            return new Entity(id, GetGeneration(id));
        }

        public bool IsAlive(in Entity entity)
        {
            return entityStores.Has(entity.id) && GetGeneration(entity.id) == entity.generation;
        }

        public void Activate(in Entity entity)
        {
            if(!IsAlive(entity))
                return;
            
            entityStores.GetRef(entity.id).Activate();
        }

        public void Deactivate(in Entity entity)
        {
            if(!IsAlive(entity))
                return;
            
            entityStores.GetRef(entity.id).Deactivate();
        }

        public void DestroyEntity(in Entity entity)
        {
            using (HashSetPool<int>.Get(out var toDestroy))
            {
                if(!IsAlive(entity))
                    return;
                
                toDestroy.Add(entity.id);

                foreach (var id in toDestroy)
                    RemoveEntity(id);
                
            }
        }

        private void RemoveEntity(int entityId)
        {
            var mask = entityStores.GetRef(entityId).Mask;

            foreach (var componentID in mask)
                componentStores[componentID].TryRemove(entityId);
            
            
            entityStores.TryRemove(entityId);

            generations[entityId]++;
            freeIds.Push(entityId);
        }
    }
}