using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Area
{
    [CreateAssetMenu(menuName = "PFE/Area/Road", fileName = "Road")]
    public class RoadAreaData : AreaData
    {
        [field: SerializeField, BoxGroup("Base")]
        public List<SpawnPoint> SpawnPoints { get; private set; }
    }
}