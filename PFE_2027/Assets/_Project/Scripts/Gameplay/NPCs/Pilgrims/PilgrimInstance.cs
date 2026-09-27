using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class PilgrimInstance : NpcInstance<PilgrimData>
    {
        public Vector3 CurrentPosition { get; private set; }

        public PilgrimInstance(PilgrimData data) : base(data)
        {
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            CurrentPosition = newPosition;
        }
    }
}
