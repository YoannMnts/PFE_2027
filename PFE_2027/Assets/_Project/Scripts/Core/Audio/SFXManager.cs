using UnityEngine;

namespace PFE.Core.Audio
{
    public class SFXManager
    {
        public void PlaySfx(AudioClip clip)
        {
            Debug.Log($"Playing SFX {clip.name}");
        }
    }
}