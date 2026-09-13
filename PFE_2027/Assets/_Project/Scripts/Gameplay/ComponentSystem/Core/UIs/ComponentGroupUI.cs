using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.Phases;
using PFE.Utilities.Scripts;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public class ComponentGroupUI : MonoBehaviour, IPhaseListener<SelectMapPhase>, IPhaseListener<ComposeBuildPhase>
    {
        [SerializeField] 
        private CanvasGroup group;

        private void Awake()
        {
            group.Hide(0.3f);
        }
        private void OnEnable()
        {
            this.Register<SelectMapPhase>();
            this.Register<ComposeBuildPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<SelectMapPhase>();
            this.Unregister<ComposeBuildPhase>();
        }

        void IPhaseListener<SelectMapPhase>.OnPhaseBegin(SelectMapPhase phase)
        {
        }

        void IPhaseListener<SelectMapPhase>.OnPhaseEnd(SelectMapPhase phase)
        {
        }

        void IPhaseListener<ComposeBuildPhase>.OnPhaseBegin(ComposeBuildPhase phase)
        {
        }

        void IPhaseListener<ComposeBuildPhase>.OnPhaseEnd(ComposeBuildPhase phase)
        {
        }
    } 
}