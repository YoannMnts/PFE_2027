using System.Collections.Generic;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public class ComponentManager
    {
        private readonly Dictionary<ComponentData, DurationController> components;
        private readonly Dictionary<ComponentData, List<ComponentData>> componentChildren;

        public ComponentManager()
        {
            components = new Dictionary<ComponentData, DurationController>();
            componentChildren = new Dictionary<ComponentData, List<ComponentData>>();
        }
        
        public bool TryAddComponent(ComponentData component)
        {
            if(components.TryAdd(component, new DurationController()) &&
               componentChildren.TryAdd(component, new List<ComponentData>()))
                return true;
            
            components.Remove(component);
            componentChildren.Remove(component);
            return false;
        }

        public bool TryAddChildTo(ComponentData component)
        {
            if(!componentChildren.TryGetValue(component, out List<ComponentData> children))
                return false;
            
            children.Add(component);
            return true;
        }

        public void TriggerAllComponents()
        {
            foreach ((var data, DurationController durationController) in components)
            {
                if(data.TryGet(out IComponentContainer container))
                    container.DecrementRechargeCount(data, durationController);
            }
            
            foreach (var (data, durationController) in components)
            {
                if (!data.TryGet(out IComponentContainer container)) 
                    continue;
                
                if(container.Trigger(data, new ComponentContext()))
                    container.StartRecharge(data, durationController);
            }
        }
    }
}