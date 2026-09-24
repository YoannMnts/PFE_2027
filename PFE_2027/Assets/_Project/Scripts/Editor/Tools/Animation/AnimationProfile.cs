using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Animation
{
    /// <summary>
    /// Asset éditeur : un personnage à animer + son projet UMotion (qui contient tous ses clips).
    /// Un profil par personnage, rangé dans son propre dossier sous Assets/_Project/Animation.
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
