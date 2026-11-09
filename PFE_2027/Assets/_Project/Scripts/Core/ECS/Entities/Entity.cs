using System;

namespace PFE.Core.Scripts.ECS
{
    public readonly struct Entity : IEquatable<Entity>
    {
        public static readonly Entity None = new Entity(-1);   // id = -1
        public bool IsValid => id >= 0;       // "not None", NOT "alive": only the World knows that

        public readonly int id;               // slot index in the World (reused after a destroy)
        public readonly int generation;       // how many times this slot was destroyed

        public Entity(int id, int generation = 0)
        {
            this.id = id;
            this.generation  = generation;
        }
        
        // id AND generation
        public bool Equals(Entity other) 
            => id == other.id && generation == other.generation;

        public override bool Equals(object obj) 
            => obj is Entity other && Equals(other);

        // combine id and generation
        public override int GetHashCode() => HashCode.Combine(id, generation);

        //Debug
        public override string ToString() 
            => $"Entity({id}, {generation})";
        
        public static bool operator ==(Entity a, Entity b) => a.Equals(b);
        public static bool operator !=(Entity a, Entity b) => !a.Equals(b);
    }
}