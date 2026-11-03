using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public partial class BattleArea : IArea<RoadAreaData>
    {
        public async Awaitable LaunchPhase(RoadAreaData data, CrossRoadGameModeContext context)
        {
            var battlePhase = new BattlePhase(context, data);
            Debug.Log($"[Battle Area] launch {data}");
            await battlePhase.Run();
        }
    }
}