using System;

namespace PFE.Core.Scripts.ECS
{
    public abstract class SparseSet<T>
    {
        public const int NONE = -1;

        public ref T this[int id] => ref GetRef(id);
        public ReadOnlySpan<T> AllElements => dense.AsSpan(0, Count);
        public ReadOnlySpan<int> AllIDs => denseToElement.AsSpan(0, Count);
        
        public int Count { get; private set; }
        
        private readonly int[] denseToElement; // Rangement => Entity
        private readonly int[] sparse; // Entity => Rangement

        private readonly T[] dense; // Rangement des composants, contigu en memoire

        public SparseSet(int capacity)
        {
            dense = new T[capacity];
            denseToElement = new int[capacity];
            sparse = new int[capacity];
            
            Array.Fill(sparse, NONE);
        }
        
        public bool TryRemove(int entityId)
        {
            if(!Has(entityId))
                return false;
            
            int index = sparse[entityId];
            
            int furthestIndex = Count - 1;
            int furthestEntityId = denseToElement[furthestIndex];
            
            //Écrase le trou avec le dernier élément
            dense[index] = dense[furthestIndex];
            denseToElement[index] = furthestEntityId;
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

        public ref T GetRef(int entityId)
        {
            if (!Has(entityId))
            {
                throw new ArgumentException($"[SparseSet] Entity {typeof(T).Name} with {entityId} does not exist");
            }
            
            return ref dense[sparse[entityId]];
        }
        
        public bool Set(int entityId, in T element)
        {
            var isNew = false;
            var index = sparse[entityId];
            
            if (index == NONE)
            {
                index = Count++;
                sparse[entityId] = index;
                denseToElement[index] = entityId;
                isNew = true;
            }
            
            dense[index] = element;
            return isNew;
        }
    }
}