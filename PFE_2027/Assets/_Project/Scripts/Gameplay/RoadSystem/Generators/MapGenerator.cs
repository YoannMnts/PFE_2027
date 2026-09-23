using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public class MapGenerator : MonoBehaviour, IPhaseListener<GenerateAreaPhase>, IPhaseListener<BattlePhase>
    {
        [SerializeField]
        private Transform container;

        private void OnEnable()
        {
            this.Register<GenerateAreaPhase>();
            this.Register<BattlePhase>();
        }

        private void OnDisable()
        {
            this.Unregister<GenerateAreaPhase>();
            this.Unregister<BattlePhase>();
        }

        void IPhaseListener<GenerateAreaPhase>.OnPhaseBegin(GenerateAreaPhase phase)
        {
            container.ClearChildren();
            
            Transform runtime = phase.areaData.Prefab.InstantiatePrefab();
            runtime.transform.SetParent(container);
            
            phase.SetResult(true);
        }

        void IPhaseListener<GenerateAreaPhase>.OnPhaseEnd(GenerateAreaPhase phase)
        {
        }

        void IPhaseListener<BattlePhase>.OnPhaseBegin(BattlePhase phase)
        {
        }

        void IPhaseListener<BattlePhase>.OnPhaseEnd(BattlePhase phase)
        {
            container.ClearChildren();
        }
    }
}