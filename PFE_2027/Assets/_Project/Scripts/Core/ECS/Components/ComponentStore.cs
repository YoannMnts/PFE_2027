using System;

namespace PFE.Core.Scripts.ECS
{
    public sealed class ComponentStore<TComponent> : IComponentStore 
        where TComponent : struct, IEntityComponent
    {
        public const int NONE = -1;
        
        public int Count { get; private set; }
        
        private readonly int[] denseToEntity; // dense[i] appartient à quelle entité ?
        private readonly int[] sparse; // entityId → index dans dense (-1 = absent)

        private TComponent[] dense; // les composants, contigus en mémoire
        
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
            int index = sparse[entityId];
            if(index == NONE)
                return false;
            
            int lastIndex = Count - 1;
            int lastEntityId = denseToEntity[lastIndex];
            
            //Écrase le trou avec le dernier élément
            dense[index] = dense[lastIndex];
            denseToEntity[index] = lastEntityId;
            sparse[lastEntityId] = index;

            //Nettoie la dernière case
            sparse[entityId] = NONE;
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
        
        public void Set(int entityId, TComponent component)
        {
            var index = sparse[entityId];
            if (index == NONE)
            {
                index = Count++;
                sparse[entityId] = index;
                denseToEntity[index] = entityId;
            }
            
            dense[index] = component;
        }
    }

    public interface IComponentStore
    {
        int Count { get; }
        bool TryRemove(int entityId);
        bool Has(int entityId);
    }
}