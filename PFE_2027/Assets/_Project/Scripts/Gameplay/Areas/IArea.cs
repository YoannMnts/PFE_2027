using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    [GenerateContainer]
    public interface IArea<in TData> : IBehaviour<TData> where TData : IAreaData
    {
        [AddToContainer]
        Awaitable LaunchPhase(TData data, CrossRoadGameModeContext context);
    }
}