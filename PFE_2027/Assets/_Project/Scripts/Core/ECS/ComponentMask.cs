using System.ComponentModel;

namespace PFE.Core.Scripts.ECS
{
    public struct ComponentMask
    {
        private ulong bits;

        public bool IsEmpty => bits == 0;

        public void Set(int id)
        {
            bits |= 1UL << id;
        }

        public void Clear(int id)
        {
            bits &= ~ 1UL << id;
        }

        public readonly bool Has(int id)
        {
            return (bits & (1UL << id)) != 0;
        }

        public readonly bool MatchesAll(in ComponentMask other)
        {
            return (bits & other.bits) == bits;
        }
        
        public readonly bool MatchesAny(in ComponentMask other)
        {
            return (bits & other.bits) != 0;
        }   

        // mask with one bit
        public static ComponentMask Of<T>() where T : struct, IComponent
        {
            
        }
        
        // returns a copy + this bit (fluent)
        public ComponentMask With<T>() where T : struct, IEntityComponent
        {
            var id = ComponentID<T>.Value;
            if(id != -1)
                Set(id);
            
            return this;
        }        

        // foreach (int id in mask): ids whose bit is 1
        public Enumerator GetEnumerator()
        {
            return new Enumerator();
        }   
    }

    public struct Enumerator
    {
    }
}