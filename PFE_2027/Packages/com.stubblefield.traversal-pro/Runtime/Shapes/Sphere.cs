using UnityEngine;

namespace TraversalPro
{
    [System.Serializable]
    internal struct Sphere
    {
        public Vector3 center;
        public float radius;
        public Vector3 Bottom => new Vector3(center.x, center.y - radius, center.z);
        public Vector3 Top => new Vector3(center.x, center.y + radius, center.z);

        public Sphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public Sphere(SphereCollider collider)
        {
            center = collider.transform.TransformPoint(collider.center);
            Vector3 scale = collider.transform.lossyScale;
            float maxScale = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
            radius = collider.radius * maxScale;
        }
    }
}