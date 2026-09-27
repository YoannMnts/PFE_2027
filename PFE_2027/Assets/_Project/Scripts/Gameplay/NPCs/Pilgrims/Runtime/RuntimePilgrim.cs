using PFE.Gameplay.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class RuntimePilgrim : RuntimeNpc<PilgrimInstance>
    {
        private void FixedUpdate()
        {
            if (instance != null)
                instance.UpdatePosition(transform.position);
        }
    }
}
