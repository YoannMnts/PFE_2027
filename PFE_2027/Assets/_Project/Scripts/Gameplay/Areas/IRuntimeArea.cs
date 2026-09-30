using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public interface IRuntimeArea
    {
        SplineContainer Path { get; }
        
        bool TryGetAnchor(string id, out Transform anchor);
    }
}
