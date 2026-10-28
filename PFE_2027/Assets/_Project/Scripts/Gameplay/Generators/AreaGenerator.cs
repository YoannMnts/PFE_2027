using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public class AreaGenerator : MonoBehaviour, IPhaseListener<GenerateAreaPhase>, IPhaseListener<CleanupPhase>
    {
        [SerializeField]
        private Transform container;

        private void OnEnable()
        {
            this.Register<CleanupPhase>();
            this.Register<GenerateAreaPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<CleanupPhase>();
            this.Unregister<GenerateAreaPhase>();
        }

        //---------------------------------------------
        
        void IPhaseListener<CleanupPhase>.OnPhaseBegin(CleanupPhase phase)
        {
            container.ClearChildren();
        }

        void IPhaseListener<CleanupPhase>.OnPhaseEnd(CleanupPhase phase)
        {
        }
        
        
        void IPhaseListener<GenerateAreaPhase>.OnPhaseBegin(GenerateAreaPhase phase)
        {
            Transform runtime = phase.areaData.AreaPrefab.InstantiatePrefab();
            runtime.transform.SetParent(container);

            if (!runtime.TryGetComponent(out IRuntimeArea arena))
                Debug.LogError($"[AreaGenerator] Area prefab '{phase.areaData.AreaPrefab.name}' has no RuntimeArena on its root.", runtime);

            phase.SetResult(arena);
        }

        void IPhaseListener<GenerateAreaPhase>.OnPhaseEnd(GenerateAreaPhase phase)
        {
        }

    }
}
