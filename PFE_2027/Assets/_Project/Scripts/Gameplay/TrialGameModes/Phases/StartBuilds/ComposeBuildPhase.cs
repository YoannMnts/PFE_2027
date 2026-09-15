using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.GameModes;
using UnityEngine;
using Random = UnityEngine.Random;

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

        private ComponentData[] GetRandomComponent(IEnumerable<ComponentData> componentDatas)
        {
            var array = componentDatas.ToArray();

            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }

            int count = Mathf.Min(10, array.Length);
            var selected = new ComponentData[count];
            Array.Copy(array, selected, count);

            return selected;
        }
    }
}