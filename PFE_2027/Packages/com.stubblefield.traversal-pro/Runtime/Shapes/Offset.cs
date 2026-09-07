using UnityEngine;

namespace TraversalPro
{
    [System.Serializable]
    public struct Offset2
    {
        Vector2 direction;
        public float Magnitude { get; set; }

        public Vector2 Direction
        {
            get => direction;
            set => direction = value.normalized;
        }

        public float X => direction.x * Magnitude;
        public float Y => direction.y * Magnitude;

        public Vector2 Value => Direction * Magnitude;

        public Offset2(float x, float y) : this(new Vector2(x, y)) { }
        
        public Offset2(Vector2 value)
        {
            if (value == default)
            {
                Magnitude = 0;
                direction = default;
            }
            else
            {
                Magnitude = value.magnitude;
                direction = value / Magnitude;
            }
        }
        
        public bool IsValid() => Magnitude > 0;
    }
    
    [System.Serializable]
    public struct Offset3
    {
        Vector3 direction;
        public float Magnitude { get; set; }

        public Vector3 Direction
        {
            get => direction;
            set => direction = value.normalized;
        }

        public float X => direction.x * Magnitude;
        public float Y => direction.y * Magnitude;
        public float Z => direction.z * Magnitude;
        
        public Vector3 Value => Direction * Magnitude;
        
        public Offset3(float x, float y, float z) : this(new Vector3(x, y, z)) { }
        
        public Offset3(Vector3 value)
        {
            if (value == default)
            {
                Magnitude = 0;
                direction = default;
            }
            else
            {
                Magnitude = value.magnitude;
                direction = value / Magnitude;
            }
        }
        
        public bool IsValid() => Magnitude > 0;
    }
}