using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Animation
{
    /// <summary>
    /// Editor asset: a character to animate + its UMotion project (holding all its clips).
    /// One profile per character, stored in its own folder under Assets/_Project/Animation.
    /// </summary>
    public sealed class AnimationProfile : ScriptableObject
    {
        [SerializeField, Tooltip("Prefab of the character animated in the stage.")]
        private GameObject character;

        [SerializeField, Tooltip("UMotion project holding every clip of this character.")]
        private Object umotionProject;

        public GameObject Character => character;
        public Object UMotionProject => umotionProject;

        internal void Initialize(GameObject character, Object umotionProject)
        {
            this.character = character;
            this.umotionProject = umotionProject;
        }
    }
}
