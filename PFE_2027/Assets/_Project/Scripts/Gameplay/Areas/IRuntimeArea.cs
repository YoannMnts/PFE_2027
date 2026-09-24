using UnityEngine;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public interface IRuntimeArea
    {
        bool TryGetAnchor(string id, out Transform anchor);
    }
}
