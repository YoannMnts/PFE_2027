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
using PFE.Gameplay.Scripts.RoadSystem;
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
        
        public bool TryAdd(NpcData data, IRuntimeEntity runtimeEntity, IRuntimeArea area, float progressionGoal = 0)
        {
            //si la data n'a pas de behavior
            if (!data.TryGet(out INpcContainer container))
                return false;
            
            var entity = World.CreateEntity();
            var entityAddress = new EntityAddress(World, entity);
            var setupContext = new EntitySetupContext(context, entityAddress);
            container.SetupEntity(data, setupContext);
            
            runtimeEntityRegistry.Set(entity.id, runtimeEntity);
            runtimeEntity.Connect(context, entityAddress);

            if (!entityAddress.TryGetComponentRef<HealthComponent>(out var healthComponent)) 
                return false;
            
            var component = healthComponent.GetValue();
            component.Health.OnValueChanged += OnHealthChanged;

            if (progressionGoal > 0)
            {
                entityAddress.AddComponent(new PendingNpcComponent(progressionGoal));
                entityAddress.Deactivate();
            }
            
            return true;
        }
        
        private void OnHealthChanged(float value)
        {
            if(value > 0)
                return;
            
        }
    }
}
