using System;
using UnityEngine;

namespace PFE.Core.Scripts.ECS
{
    public static class ComponentRegistry
    {
        public const int MAX_COMPONENTS = 64;
        
        private static int index;
        private static readonly Type[] IDMapping;

        static ComponentRegistry()
        {
            index = 0;
            IDMapping = new Type[MAX_COMPONENTS];
        }
        
        internal static int Next<T>() where T : IEntityComponent
        {
            var next = index++;
            
            if (next >= IDMapping.Length)
                throw new InvalidOperationException($"Too many component types (max {MAX_COMPONENTS}): {typeof(T).Name}");
            
            IDMapping[next] = typeof(T);
            return next;
        }
        
        
    }
}