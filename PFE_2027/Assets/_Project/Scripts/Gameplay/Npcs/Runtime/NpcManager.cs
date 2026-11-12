using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Entities;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    public class NpcManager
    {
        private readonly CrossRoadGameModeContext context;
        public World World => context.gameMode.World;
        
        public NpcManager(CrossRoadGameModeContext context)
        {
            this.context = context;
        }

        public readonly RuntimeEntityRegistry runtimeEntityRegistry = new(64);
        
        public bool TryAdd(NpcData data, IRuntimeEntity runtimeEntity)
        {
            //si la data n'a pas de behavior
            if (!data.TryGet(out INpcContainer container))
                return false;
            
            var entity = World.CreateEntity();
            var entityAddress = new EntityAddress(World, entity);
            container.SetupEntity(data, entityAddress);
            
            runtimeEntityRegistry.Set(entity.id, runtimeEntity);
            runtimeEntity.Connect(context, entityAddress);

            if (!entityAddress.TryGetComponentRef<HealthComponent>(out var healthComponent)) 
                return true;
            
            var component = healthComponent.GetValue();
            component.Health.OnValueChanged += OnHealthChanged;

            return true;
        }
        
        private void OnHealthChanged(float value)
        {
            if(value > 0)
                return;
            
        }
    }
}
