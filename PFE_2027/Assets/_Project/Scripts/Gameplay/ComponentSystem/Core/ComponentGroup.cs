using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public class ComponentGroup
    {
        private sealed class ComponentInstance
        {
            public readonly ComponentData data;

            // Null for a family with no recharge concept (Element, Passive) — see TryAddComponent.
            [CanBeNull] public readonly DurationController durationController;
            public readonly List<ComponentData> children;

            public ComponentInstance(ComponentData data, DurationController durationController, List<ComponentData> children)
            {
                this.data = data;
                this.durationController = durationController;
                this.children = children;
            }
        }

        public ComponentGroup(ComponentGroupData data)
        {
            ComponentGroupData = data;
        }
        
        public ComponentGroupData ComponentGroupData { get; private set; }
        
        private readonly Dictionary<ComponentInstanceId, ComponentInstance> instances = new();
        private int nextId;

        
        public ComponentInstanceId TryAddComponent(ComponentData data)
        {
            var id = new ComponentInstanceId(nextId++);

            // Only BasicAttack/SubAttack data carries a recharge time; everything else
            // gets no DurationController at all instead of one nobody ever uses.
            var rechargeTime = data is RechargeableComponentData rechargeable ? 
                new DurationController(rechargeable.MaxRechargeValue)
                : null;

            instances.Add(
                id,
                new ComponentInstance(data, rechargeTime, new List<ComponentData>())
                );

            return id;
        }

        public bool TryAddChildTo(ComponentInstanceId id, ComponentData child)
        {
            if (!instances.TryGetValue(id, out var instance))
                return false;

            instance.children.Add(child);
            return true;
        }

        public void TriggerAllComponents(IPlayer player, Action<ComponentData> onComponentTrigger)
        {
            foreach (var instance in instances.Values)
            {
                if (instance.durationController != null
                    && instance.data.TryGet(out IRechargeableComponentContainer container))
                    container.DecrementRechargeCount((RechargeableComponentData)instance.data, instance.durationController);
            }

            foreach (var instance in instances.Values)
            {
                if (instance.data.TryGet(out IComponentContainer container))
                {
                    // rechargeTime is null here for Element/Passive — their CanTrigger/Trigger
                    // must not assume context.durationController is set.
                    container.Trigger(instance.data, new ComponentContext(instance.durationController, player));
                    onComponentTrigger?.Invoke(instance.data);
                }
            }
        }

        public void Clear()
        {
            instances.Clear();
            nextId = 0;
        }
    }
}
