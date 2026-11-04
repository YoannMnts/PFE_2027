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
        public readonly struct DataInstanceNpc : IEquatable<DataInstanceNpc>
        {
            public readonly INpcData data;
            public readonly INpcInstance instance;
            
            public DataInstanceNpc(INpcData data, INpcInstance instance)
            {
                this.data = data;
                this.instance = instance;
            }

            public bool Equals(DataInstanceNpc other)
            {
                return instance == other.instance;
            }
        }
        
        public event Action<INpcInstance> OnNpcAdded;
        public event Action<INpcInstance> OnNpcDied;

        //créer une liste vide (à la création du Manager) des NpcInstance qui ne peut etre effacer mais peut etre modifier 
        private readonly List<DataInstanceNpc> npcList = new();
        private readonly List<DataInstanceNpc> deadNpcList = new();
        
        public int Count => npcList.Count;

        //------------------- Add to List ---------------------
        
        // One instance per call, i.e. per spawned NPC: two NPCs sharing the same data get two distinct instances.
        // The created instance is handed back so the caller plugs it into its own runtime, without any lookup
        public bool TryAdd(NpcData data, NpcInstanceContext context, out INpcInstance instance)
        {
            //si la data n'a pas de behavior
            if (!data.TryGet(out INpcContainer container))
            {
                instance = null;
                return false;
            }
            
            //création de l'instance
            instance = container.CreateInstance(data, context);
            if (instance == null)
            {
                Debug.LogError($"[Npc Manager] '{data.GetType().Name}' behaviour created a null instance.");
                return false;
            }

            //si l'instance meurt on l'a remove auto des npcs
            instance.OnDeath += OnInstanceDeath;
            
            //ajout dans la struct de stockage => ajout dans la liste
            npcList.Add(new DataInstanceNpc(data, instance));
            
            OnNpcAdded?.Invoke(instance);
            return true;
        }
        
        
        //----------------------------Try Get ------------------------
        
        public bool TryGetFirstInstance<TInstance>(out TInstance result) 
            where TInstance : class, INpcInstance
        {
            foreach (DataInstanceNpc instance in npcList)
            {
                if (instance.instance is not TInstance typedInstance)
                    continue;
                result = typedInstance;
                return true;
            }
            result = null;
            return false;
        }
        
        public bool TryGetFirstData<TData>(out TData pilgrimData)
            where TData : NpcData
        {
            foreach (DataInstanceNpc data in npcList)
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
            Debug.LogError($"[Npc Manager] No instance has been found for {data.Name}");
            instance = null;
            return false;
        }
        
        //--------------------- Remove ----------------------
        
        // Takes the NPC out of the registry without counting it as dead (e.g. its runtime couldn't be set up)
        public bool TryRemove(INpcInstance instance)
        {
            if (!TryGetDataInstance(instance, out var dataInstance))
            {
                return false;
            }

            instance.OnDeath -= OnInstanceDeath;
            npcList.Remove(dataInstance);
            return true;
        }

        // Subscribed to every registered instance: only the instance that died leaves the list
        private void OnInstanceDeath(INpcInstance instance)
        {
            if (!TryGetDataInstance(instance, out var dataInstance) 
                || !TryRemove(instance))
            {
                return;
            }
            
            deadNpcList.Add(dataInstance);
            OnNpcDied?.Invoke(instance);
        }

        // Compared by reference: the exact instance, never another one sharing its data
        private bool TryGetDataInstance(INpcInstance instance, out DataInstanceNpc dataInstanceNpc)
        {
            if (instance == null)
            {
                dataInstanceNpc = new DataInstanceNpc();
                return false;   
            }

            for (int i = 0; i < npcList.Count; i++)
            {
                DataInstanceNpc currentDataInstanceNpc = npcList[i];

                if (currentDataInstanceNpc.instance == instance)
                {
                    dataInstanceNpc = currentDataInstanceNpc;
                    return true;
                }
            }
            
            dataInstanceNpc = new DataInstanceNpc();
            return false;
        }
        
        public void Clear()
        {
            foreach (var dataInstance in npcList)
            {
                dataInstance.instance.OnDeath -= OnInstanceDeath;
                OnNpcDied?.Invoke(dataInstance.instance);
                dataInstance.instance.Dying();
            }
            
            npcList.Clear();
        }
    }
}
