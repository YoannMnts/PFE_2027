using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace PFE.Core.Scripts.ECS
{
    public partial class World
    {
        public ReadOnlySpan<EntityMeta> Metas => entities.AllElements;
        public ReadOnlySpan<int> Entities => entities.AllIDs;
        
        private readonly int maxEntities;
        
        private readonly IComponentStore[] stores;
        private readonly SparseSet<EntityMeta> entities;
        private readonly int[] generations;
        
        private readonly Stack<int> freeIds = new ();
        private int nextId;
        
        public World(int maxEntities)
        {
            this.maxEntities = maxEntities;
            stores = new IComponentStore[maxEntities];
            entities = new SparseSet<EntityMeta>(maxEntities);
            generations = new int[maxEntities];
        }
        
        public int GetGeneration(int id) => id >= 0 && id < generations.Length ? generations[id] : 0;

        public ComponentStore<T> GetOrCreateStore<T>() where T : struct, IEntityComponent
        {
            var id = ComponentID<T>.Value;
            
            if(stores[id] is ComponentStore<T> store)
                return store;
            
            var newStore = new ComponentStore<T>(maxEntities);
            stores[id] = newStore;
            return newStore;
        }

        public bool TryGetStore<T>(out ComponentStore<T> store) where T : struct, IEntityComponent
        {
            var componentStore = stores[ComponentID<T>.Value];
            
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
            if(nextId >= maxEntities)
                throw new InvalidOperationException($"[World] Capacity of {maxEntities} entities exceeded.");
            
            var id = freeIds.Count > 0 ? freeIds.Pop() : nextId++;
            entities.Set(id, new EntityMeta(true));
            return new Entity(id, GetGeneration(id));
        }

        public bool IsAlive(in Entity entity)
        {
            return entities.Has(entity.id) && GetGeneration(entity.id) == entity.generation;
        }

        public void Activate(in Entity entity)
        {
            if(!IsAlive(entity))
                return;
            
            entities.GetRef(entity.id).Activate();
        }

        public void Deactivate(in Entity entity)
        {
            if(!IsAlive(entity))
                return;
            
            entities.GetRef(entity.id).Deactivate();
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
            var mask = entities.GetRef(entityId).Mask;

            foreach (var componentID in mask)
                stores[componentID].TryRemove(entityId);
            
            
            entities.TryRemove(entityId);

            generations[entityId]++;
            freeIds.Push(entityId);
        }
    }
}