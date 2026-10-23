using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // PAS DE SINGLETON
    // Registry of the battle's living NPCs, subscribes to their death to stay up to date
    public class NpcManager
    {
        public readonly struct DataInstance
        {
            public readonly INpcData data;
            public readonly INpcInstance instance;
            
            public DataInstance(INpcData data, INpcInstance instance)
            {
                this.data = data;
                this.instance = instance;
            }
        }
        
        public event Action<INpcInstance> OnNpcAdded;
        public event Action<INpcInstance> OnNpcDied;

        //créer une liste vide (à la création du Manager) des NpcInstance qui ne peut etre effacer mais peut etre modifier 
        private readonly List<DataInstance> npcList = new();
        private readonly List<DataInstance> deadNpcList = new();
        
        public int Count => npcList.Count;

        public bool TryGetFirst<T>(out T result) where T : class, INpcInstance
        {
            foreach (DataInstance instance in npcList)
            {
                if (instance.instance is not T typedInstance)
                    continue;
                result = typedInstance;
                return true;
            }
            result = null;
            return false;
        }
        
        
        public bool TryGetData(out PilgrimData pilgrimData)
        {
            foreach (DataInstance data in npcList)
            {
                if (data.data is PilgrimData compatible)
                {
                    pilgrimData = compatible;
                    return true;
                }
            }
            pilgrimData = null;
            return false;
        }
        
        //méthode permettant de récupérer une instance par rapport à une data
        public bool TryGetInstance(INpcData data, out INpcInstance instance)
        {
            foreach (var dataInstance in npcList)
            {
                if (dataInstance.data == data)
                {
                    instance = dataInstance.instance;
                    return true;
                }
            }
            Debug.LogError($"[Npc Manager] No instance has been found for  {data.Name}");
            instance = null;
            return false;
        }
        
        
        //il creé, associe et ajoute dans le dictionnaire
        public void Add(NpcData data, NpcInstanceContext context)
        {
            //si la data n'a pas de behavior
            if (!data.TryGet(out INpcContainer container))
                return;
            
            //création de l'instance
            INpcInstance newInstance = container.CreateInstance(data, context);
            
            //ajout dans la struct de stockage => ajout dans la liste
            var dataInstance = new DataInstance(data, newInstance);
            npcList.Add(dataInstance);
            
            OnNpcAdded?.Invoke(newInstance);
        }

        public void Remove(INpcData data)
        {
            for (int i = 0; i < npcList.Count; i++)
            {
                if (npcList[i].data == data)
                {
                    npcList.RemoveAt(i);
                    
                    Debug.LogWarning($"[Npc Manager] data || instance was removed from {npcList}");
                }
                Debug.LogWarning($"[Npc Manager] data || instance wasn't removed from {npcList}");
                return;
            }
        }
        
        public void Clear()
        {
            npcList.Clear();
        }
    }
}
