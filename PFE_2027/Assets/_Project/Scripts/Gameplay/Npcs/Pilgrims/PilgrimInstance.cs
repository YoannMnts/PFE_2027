using System;
using System.Collections.Generic;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;
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
        
        public float Progression { get; private set; }
        
        private IEnumerable<IPlayer> players;
        public event Action<Vector3> OnPositionChanged;
        

        public PilgrimInstance(PilgrimData data, SplineContainer path, IEnumerable<IPlayer> players) : base(data)
        {
            this.path = path;
            this.players = players;
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            CurrentPosition = newPosition;

            TargetDestination = GetPositionFromSpline(newPosition, data.TargetPositionDistance);
            SetGoal(TargetDestination);
            
            OnPositionChanged?.Invoke(CurrentPosition);
        }

        private float3 GetPositionFromSpline(Vector3 newPosition, float distance)
        {
            Spline spline =  path.Spline;
            var localPosition = path.transform.InverseTransformPoint(newPosition);
            SplineUtility.GetNearestPoint(spline, localPosition, out float3 _, out float nearestPoint);
            
            Progression = nearestPoint;
            
            spline.GetPointAtLinearDistance(nearestPoint, distance, out float localTargetPosition);
            
            return path.EvaluatePosition(spline, localTargetPosition);;
        }
        
    }
}
