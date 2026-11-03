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
            bits &= ~ (1UL << id);
        }

        public readonly bool Has(int id)
        {
            return (bits & (1UL << id)) != 0;
        }

        public readonly bool MatchesAll(in ComponentMask other)
        {
            return (bits & other.bits) == other.bits;
        }
        
        public readonly bool MatchesAny(in ComponentMask other)
        {
            return (bits & other.bits) != 0;
        }   

        // mask with one bit
        public static ComponentMask Of<T>() where T : struct, IEntityComponent
        {
            var mask = new ComponentMask();
            mask.Set(ComponentID<T>.Value);
            return mask;
        }
        
        // returns a copy + this bit (fluent)
        public readonly ComponentMask With<T>() where T : struct, IEntityComponent
        {
            var copy = this;
            
            var componentID = ComponentID<T>.Value;
            copy.Set(componentID);
            
            return copy;
        }        

        // foreach (int id in mask): ids whose bit is 1
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        
        public struct Enumerator
        {
            public int Current => index;
        
            private readonly ComponentMask mask;
            private int index;

            public Enumerator(ComponentMask mask)
            {
                this.mask = mask;
                index = -1;
            }

            public bool MoveNext()
            {
                while(++index < 64)
                    if(mask.Has(index))
                        return true;
            
                return false;
            }
        }
    }

    
}