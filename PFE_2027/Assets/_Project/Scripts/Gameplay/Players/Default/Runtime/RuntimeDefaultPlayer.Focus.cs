using PFE.Gameplay.Scripts.Enemy.Runtime;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer
    {
        private RuntimeEnemy currentFocusTarget;
        
        public void StartFocusMode()
        {
            if(!TryGetNearestInteractable<RuntimeEnemy>(out var enemy))
                return;

            currentFocusTarget = enemy.Interactable;
        }

        private void UpdateFocus()
        {
            
        }
    }
}