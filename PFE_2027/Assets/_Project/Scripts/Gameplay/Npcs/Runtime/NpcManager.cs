using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.Entities;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    public class NpcManager
    {
        private readonly World world;
        
        public NpcManager(World world)
        {
            this.world = world;
        }

        private readonly RuntimeEntityRegistry runtimeEntityRegistry = new(64);
        
        public bool TryAdd(NpcData data, IRuntimeEntity runtimeEntity)
        {
            //si la data n'a pas de behavior
            if (!data.TryGet(out INpcContainer container))
                return false;
            
            var entity = world.CreateEntity();
            var entityAddress = new EntityAddress(world, entity);
            container.SetupEntity(data, entityAddress);
            
            runtimeEntityRegistry.Set(entity.id, runtimeEntity);

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
