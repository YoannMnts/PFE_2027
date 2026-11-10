using PFE.Core.Scripts.ECS;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.Components
{
    public struct FollowSplineComponent : IEntityComponent
    {
        public SplineContainer SplineContainer { get; private set; }
        public float Progression { get; private set; }

        
        public FollowSplineComponent(SplineContainer splineContainer)
        {
            SplineContainer = splineContainer;
            Progression = 0;
        }
        
        private float3 GetPositionFromSpline(Vector3 newPosition, float distance)
        {
            Spline spline =  SplineContainer.Spline;
            var localPosition = SplineContainer.transform.InverseTransformPoint(newPosition);
            SplineUtility.GetNearestPoint(spline, localPosition, out float3 _, out float nearestPoint);
            
            Progression = nearestPoint;
            
            spline.GetPointAtLinearDistance(nearestPoint, distance, out float localTargetPosition);
            
            return SplineContainer.EvaluatePosition(spline, localTargetPosition);
        }
    }
}