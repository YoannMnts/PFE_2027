using System.Collections.Generic;
using Transform = UnityEngine.Transform;

namespace PFE.Core.Scripts.Area
{
    public interface IAreaData
    {
        public Transform AreaPrefab { get;  }
        public List<SpawnPoint> SpawnPoints { get;  }

    }
}