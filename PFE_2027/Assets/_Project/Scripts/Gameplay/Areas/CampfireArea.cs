using System.Data;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using UnityEngine;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public partial class CampfireArea : IArea<CampfireAreaData>
    {
        public async Awaitable LaunchPhase(CampfireAreaData data, CrossRoadGameModeContext context)
        {
            var campfirePhase = new CampfirePhase(context, data);
            await campfirePhase.Run();         
        }
    }
}