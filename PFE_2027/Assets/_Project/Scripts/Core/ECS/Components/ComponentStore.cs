using System;

namespace PFE.Core.Scripts.ECS
{
    public sealed class ComponentStore<TComponent> : IComponentStore 
        where TComponent : struct, IEntityComponent
    {
        public const int NONE = -1;

        public ReadOnlySpan<TComponent> AllComponents => dense.AsSpan(0, Count);
        public ReadOnlySpan<int> AllEntities => denseToEntity.AsSpan(0, Count);
        
        public int Count { get; private set; }
        
        private readonly int[] denseToEntity; // Rangement => Entity
        private readonly int[] sparse; // Entity => Rangement

        private TComponent[] dense; // Rangement des composants, contigu en memoire
        
        public ComponentStore(int maxEntities)
        {
            dense = new TComponent[maxEntities];
            denseToEntity = new int[maxEntities];
            sparse = new int[maxEntities];
            Array.Fill(sparse, NONE);
            
            Count = 0;
        }
        
        public void Add(int entityId)
        {
            var component = new TComponent();
            Set(entityId, component);
        }

        public bool TryRemove(int entityId)
        {
            if(!Has(entityId))
                return false;
            
            int index = sparse[entityId];
            
            int furthestIndex = Count - 1;
            int furthestEntityId = denseToEntity[furthestIndex];
            
            //Écrase le trou avec le dernier élément
            dense[index] = dense[furthestIndex];
            denseToEntity[index] = furthestEntityId;
            sparse[furthestEntityId] = index;

            //Nettoie la dernière case
            sparse[entityId] = NONE;
            dense[furthestIndex] = default;
            Count--;
            
            return true;
        }

        public bool Has(int entityId)
        {
            if (entityId < 0 || entityId >= sparse.Length)
                return false;
            
            return sparse[entityId] != NONE;
        }

        public ref TComponent GetRef(int entityId)
        {
            if (!Has(entityId))
            {
                throw new ArgumentException($"[ComponentStore] Entity {typeof(TComponent).Name} with {entityId} does not exist");
            }
            
            return ref dense[sparse[entityId]];
        }
        
        public bool Set(int entityId, in TComponent component)
        {
            var isEmpty = true;
            var index = sparse[entityId];
            
            if (index == NONE)
            {
                index = Count++;
                sparse[entityId] = index;
                denseToEntity[index] = entityId;
                isEmpty = false;
            }
            
            dense[index] = component;
            return isEmpty;
        }
    }

    public interface IComponentStore
    {
        int Count { get; }
        bool TryRemove(int entityId);
        bool Has(int entityId);
    }
}