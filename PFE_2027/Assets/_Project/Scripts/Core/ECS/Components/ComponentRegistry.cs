using System;
using UnityEngine;

namespace PFE.Core.Scripts.ECS
{
    public static class ComponentRegistry
    {
        public const int MAX_COMPONENTS = 64;
        
        private static int index;
        private static Type[] idMapping;

        static ComponentRegistry()
        {
            index = 0;
            idMapping = new Type[MAX_COMPONENTS];
        }
        
        internal static int Next<T>() where T : IEntityComponent
        {
            var next = index++;
            
            if (next >= MAX_COMPONENTS)
                throw new InvalidOperationException($"Too many component types (max {MAX_COMPONENTS}): {typeof(T).Name}");
            
            idMapping[next] = typeof(T);
            return next;
        }
        
        
    }
}