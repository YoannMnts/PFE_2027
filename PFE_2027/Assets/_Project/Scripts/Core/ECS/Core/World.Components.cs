using System;
using UnityEngine;

namespace PFE.Core.Scripts.ECS
{
    public partial class World
    {
        public bool AddComponent<T>(Entity entity, in T component) where T : struct, IEntityComponent
        {
            if (!IsAlive(entity))
            {
                Debug.LogError($"[World] {entity.ToString()} is not alive]");
                return false;
            }

            var store = GetOrCreateStore<T>();
            var isNew = store.Set(entity.id, component);

            if (isNew)
            {
                ref var meta = ref entityStores.GetRef(entity.id);
                meta.AddComponent<T>();
            }
            
            return isNew;
        }
        
        public bool HasComponent<T>(Entity entity) where T : struct, IEntityComponent
        {
            if (!IsAlive(entity))
                return false;
            
            var meta = entityStores.GetRef(entity.id);
            return meta.Mask.Has(ComponentID<T>.Value);
        }

        public bool TryGetComponent<T>(Entity entity, out T component) where T : struct, IEntityComponent
        {
            if (!TryGetComponentRef<T>(entity, out var componentRef))
            {
                component = default;
                return false;
            }

            component = componentRef;
            return true;
        }
        
        public bool TryGetComponentRef<T>(Entity entity, out ComponentRef<T> componentRef) where T : struct, IEntityComponent
        {
            if (!IsAlive(entity))
            {
                componentRef = default;
                return false;
            }

            if (!TryGetStore<T>(out var store))
            {
                componentRef = default;
                return false;
            }
            
            if (!HasComponent<T>(entity))
            {
                componentRef = default;
                return false;
                
            }

            componentRef = new ComponentRef<T>(entity, store, this);
            return true;
        }
        
        public bool RemoveComponent<T>(Entity entity) where T : struct, IEntityComponent
        {
            if (!IsAlive(entity))
            {
                Debug.LogError($"[World] {entity.ToString()} is not alive]");
                return false;
            }

            if (!HasComponent<T>(entity))
            {
                Debug.LogError($"[World] Entity {entity.ToString()} does not have a component {typeof(T)}");
                return false;
            }

            if (!TryGetStore<T>(out var store))
            {
                Debug.LogError($"[World] Store {typeof(T)} not found");
                return false;
            }

            if (!store.TryRemove(entity.id))
            {
                Debug.LogError($"[World] Can't remove {entity.ToString()} from store {typeof(T)}");
                return false;
            }
            
            ref var meta = ref entityStores.GetRef(entity.id);
            meta.RemoveComponent<T>();
            
            return true;
        }
    }
}