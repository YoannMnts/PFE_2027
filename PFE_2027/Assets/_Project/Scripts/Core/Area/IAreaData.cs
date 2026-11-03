using System.Collections.Generic;
using PFE.Core.Scripts.DataMapping.Interfaces;
using Transform = UnityEngine.Transform;

namespace PFE.Core.Scripts.Area
{
    public interface IAreaData : IData
    {
        public Transform AreaPrefab { get;  }
        public List<SpawnPoint> SpawnPoints { get;  }

    }
}