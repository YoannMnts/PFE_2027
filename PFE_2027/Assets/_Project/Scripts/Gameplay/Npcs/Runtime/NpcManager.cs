using System;
using System.Collections.Generic;

namespace PFE.Gameplay.Scripts.NPCs
{
    // PAS DE SINGLETON
    // Registry of the battle's living NPCs, subscribes to their death to stay up to date
    public class NpcManager
    {
        public event Action<INpcInstance> OnNpcAdded;
        public event Action<INpcInstance> OnNpcDied;

        //créer une liste vide (à la création du Manager) des NpcInstance qui ne peut etre effacer mais peut etre modifier 
        private readonly List<INpcInstance> npcs = new();
        
        //associe la liste au dessus mais en "vu seulement" pour que les autres scripts voient mais n'y touche pas 
        public IReadOnlyList<INpcInstance> Npcs => npcs;
        public int Count => npcs.Count;
        
        
        public void Add(INpcInstance instance)
        {
            //si instance donné est null OU si la liste contient deja d'instance
            if (instance == null || npcs.Contains(instance))
                return;

            npcs.Add(instance);
            instance.OnDeath += HandleDeath;
            OnNpcAdded?.Invoke(instance);
        }

        public bool Remove(INpcInstance instance)
        {
            if (instance == null || !npcs.Remove(instance))
                return false;

            instance.OnDeath -= HandleDeath;
            return true;
        }

        public bool TryGetFirst<T>(out T result) where T : class, INpcInstance
        {
            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i] is not T typed) 
                    continue;
                
                result = typed;
                return true;
            }

            result = null;
            return false;
        }

        public int CountOf<T>() where T : class, INpcInstance
        {
            int count = 0;
            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i] is T)
                    count++;
            }
            return count;
        }

        public void Clear()
        {
            for (int i = 0; i < npcs.Count; i++)
                npcs[i].OnDeath -= HandleDeath;

            npcs.Clear();
        }

        private void HandleDeath(INpcInstance instance)
        {
            Remove(instance);
            OnNpcDied?.Invoke(instance);
        }
    }
}
