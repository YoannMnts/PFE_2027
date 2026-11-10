namespace PFE.Core.Scripts.ECS
{
    public class ComponentID<T> where T : struct, IEntityComponent
    {
        public static readonly int Value = ComponentRegistry.Next<T>();
    }
}