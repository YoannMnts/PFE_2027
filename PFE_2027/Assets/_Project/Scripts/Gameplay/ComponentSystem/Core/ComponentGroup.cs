using System;
using System.Collections.Generic;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public class ComponentGroup
    {
        /// <summary>
        /// Handle to a component instance registered in a <see cref="ComponentGroup"/>.
        /// Do not construct this yourself — it has no meaning unless it came from
        /// <see cref="TryAddComponent"/>, the only method that registers a matching
        /// entry in <see cref="instances"/>. A hand-made id just won't match anything
        /// and every lookup against it will fail (safely).
        /// </summary>
        public readonly struct ComponentInstanceId : IEquatable<ComponentInstanceId>
        {
            public readonly int value;

            /// <summary>
            /// Do not call directly — use <see cref="ComponentGroup.TryAddComponent"/> instead.
            /// This constructor only stays public because C# has no way to restrict it to a
            /// single method of the containing type; using it elsewhere produces an id that
            /// matches no registered instance.
            /// </summary>
            public ComponentInstanceId(int value) => this.value = value;

            public bool Equals(ComponentInstanceId other) => value == other.value;
            public override bool Equals(object obj) => obj is ComponentInstanceId other && Equals(other);
            public override int GetHashCode() => value;
        }

        private sealed class ComponentInstance
        {
            public readonly ComponentData data;

            // Null for a family with no recharge concept (Element, Passive) — see TryAddComponent.
            public readonly DurationController rechargeTime;
            public readonly List<ComponentData> children;

            public ComponentInstance(ComponentData data, DurationController rechargeTime, List<ComponentData> children)
            {
                this.data = data;
                this.rechargeTime = rechargeTime;
                this.children = children;
            }
        }

        private readonly Dictionary<ComponentInstanceId, ComponentInstance> instances = new();
        private int nextId;

        public ComponentInstanceId TryAddComponent(ComponentData data)
        {
            var id = new ComponentInstanceId(nextId++);

            // Only BasicAttack/SubAttack data carries a recharge time; everything else
            // gets no DurationController at all instead of one nobody ever uses.
            var rechargeTime = data is RechargeableComponentData rechargeable
                ? new DurationController(rechargeable.MaxRechargeValue)
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

        public void TriggerAllComponents()
        {
            foreach (var instance in instances.Values)
            {
                if (instance.rechargeTime != null && instance.data.TryGet(out IRechargeableComponentContainer container))
                    container.DecrementRechargeCount((RechargeableComponentData)instance.data, instance.rechargeTime);
            }

            foreach (var instance in instances.Values)
            {
                if (instance.data.TryGet(out IComponentContainer container))
                    // rechargeTime is null here for Element/Passive — their CanTrigger/Trigger
                    // must not assume context.durationController is set.
                    container.Trigger(instance.data, new ComponentContext(instance.rechargeTime));
            }
        }

        public void Clear()
        {
            instances.Clear();
            nextId = 0;
        }
    }
}
