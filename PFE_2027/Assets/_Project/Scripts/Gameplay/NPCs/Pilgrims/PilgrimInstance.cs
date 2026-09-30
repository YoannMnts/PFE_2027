using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class PilgrimInstance : NpcInstance<PilgrimData>
    {
        private readonly SplineContainer path;
        public Vector3 CurrentPosition { get; private set; }
        public Vector3 TargetDestination { get;  private set; }

        public PilgrimInstance(PilgrimData data, SplineContainer path) : base(data)
        {
            this.path = path;
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            CurrentPosition = newPosition;

            TargetDestination = GetPositionFromSpline(newPosition, data.TargetPositionDistance);
        }

        private float3 GetPositionFromSpline(Vector3 newPosition, float distance)
        {
            Spline spline =  path.Spline;
            var localPosition = path.transform.InverseTransformPoint(newPosition);
            SplineUtility.GetNearestPoint(spline, localPosition, out float3 _, out float nearestPoint);
            
            spline.GetPointAtLinearDistance(nearestPoint, distance, out float localTargetPosition);
            
            return path.EvaluatePosition(spline, localTargetPosition);
        }
    }
}
