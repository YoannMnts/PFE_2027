using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.GameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class ComposeBuildPhase : Phase
    {
        private readonly TrialGameModeContext context;

        public ComposeBuildPhase(TrialGameModeContext context)
        {
            this.context = context;
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            var componentDatas = GameController.GameDatabase.GetAll<ComponentData>();
            
            var selectedComponentDatas = GetRandomComponent(componentDatas);
            
            var fillGroupComponent = new FillComponentGroupPhase(selectedComponentDatas);
            var result = await fillGroupComponent.Run();
        }

        private IEnumerable<ComponentData> GetRandomComponent(IEnumerable<ComponentData> componentDatas)
        {
            var selectedDatas = new ComponentData[10];
            var array = componentDatas.ToArray();

            for (int i = 0; i < selectedDatas.Length; i++)
            {
                var randomIndex = Random.Range(0, array.Length);
                while (array[randomIndex] != null)
                {
                    randomIndex = Random.Range(0, array.Length);
                }
                
                selectedDatas[i] = array[randomIndex];
                array[randomIndex] = null;
            }
            
            return selectedDatas;
        }
    }
}