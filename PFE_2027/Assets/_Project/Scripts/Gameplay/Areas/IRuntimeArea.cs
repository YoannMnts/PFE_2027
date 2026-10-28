using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.Players.Default;
using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public interface IRuntimeArea
    {
        Transform SpawnPosition { get; }
        SplineContainer Path { get; }
        
        bool TryGetAnchor(string id, out Transform anchor);
    }
}
