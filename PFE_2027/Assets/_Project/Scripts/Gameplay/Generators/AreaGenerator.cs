using Helteix.ChanneledProperties.Priorities;
using Helteix.Singletons.SceneServices;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public class AreaGenerator : MonoBehaviour, IPhaseListener<GenerateAreaPhase>, IPhaseListener<CleanupPhase>
    {
        [SerializeField]
        private Transform container;
        
        private AudioSource audioSource;

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

            var currentSfx = phase.areaData.GlobalSfx;
            PriorityTags priority = GameMetricsSettings.Current.AreaPriority;
            
            GameController.MusicManager.AddMusic(currentSfx, gameObject, (int)priority, phase.areaData.GlobalSfxVolume);

            if (!runtime.TryGetComponent(out IRuntimeArea arena))
                Debug.LogError($"[AreaGenerator] Area prefab '{phase.areaData.AreaPrefab.name}' has no RuntimeArena on its root.", runtime);

            phase.SetResult(arena);
        }

        void IPhaseListener<GenerateAreaPhase>.OnPhaseEnd(GenerateAreaPhase phase)
        {
            GameController.MusicManager.RemoveMusic(gameObject);
        }
    }
}
