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

        public bool TryGetFirstInstance<TInstance>(out TInstance result) 
            where TInstance : class, INpcInstance
        {
            foreach (DataInstance instance in npcList)
            {
                if (instance.instance is not TInstance typedInstance)
                    continue;
                result = typedInstance;
                return true;
            }
            result = null;
            return false;
        }
        
        //passé en generique mais honnetement il y en a pas tellement besoin
        public bool TryGetFirstData<TData>(out TData pilgrimData)
            where TData : NpcData
        {
            foreach (DataInstance data in npcList)
            {
                if (data.data is TData compatible)
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

            //si l'instance meurt on l'a remove auto des npcs
            newInstance.OnDeath += Remove;
            
            //ajout dans la struct de stockage => ajout dans la liste
            var dataInstance = new DataInstance(data, newInstance);
            npcList.Add(dataInstance);
            
            OnNpcAdded?.Invoke(newInstance);
        }

        public void Remove(INpcData data)
        {
            //premier parcours pour savoir qui est mort
            var removeIndex = new List<int>();
            for (int i = 0; i < npcList.Count; i++)
            {
                var dataInstance = npcList[i];
                if (dataInstance.data == data)
                {
                    dataInstance.instance.OnDeath -= Remove;
                    OnNpcDied?.Invoke(dataInstance.instance);
                    
                    deadNpcList.Add(dataInstance);
                    removeIndex.Add(i);
                    
                    Debug.LogWarning($"[Npc Manager] data || instance was removed from {npcList}");
                }
                Debug.LogWarning($"[Npc Manager] data || instance wasn't removed from {npcList}");
                return;
            }

            //deuxieme parcour pour retirer les morts de la list
            foreach (var i in removeIndex)
                npcList.RemoveAt(i);
        }
        
        public void Clear()
        {
            foreach (var dataInstance in npcList)
            {
                dataInstance.instance.OnDeath -= Remove;
                OnNpcDied?.Invoke(dataInstance.instance);
            }
            
            npcList.Clear();
        }
    }
}
