using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
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
        private ChannelKey key;

        public ComposeBuildPhase(TrialGameModeContext context)
        {
            this.context = context;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in context.trialGameMode.Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.High, true);
            }
            return base.Initialize(token);
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            
            
            var componentDatas = GameController.GameDatabase.GetAll<ComponentData>();
            
            var selectedComponentDatas = GetRandomComponent(componentDatas);
            
            var fillGroupComponent = new FillComponentGroupPhase(context,selectedComponentDatas);
            var result = await fillGroupComponent.Run();
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in context.trialGameMode.Players)
            {
                player.ShowUI.RemovePriority(key);
            }
            return base.Dispose(token);
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