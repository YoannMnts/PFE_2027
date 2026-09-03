using PFE.Utilities.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace PFE.Core.Scripts.GameSettings
{
    public class SceneLoaderUI : MonoBehaviour
    {
        [SerializeField] 
        private CanvasGroup group;
        
        [SerializeField] 
        private GameObject root;
        
        [SerializeField, Range(0f, 10f)] 
        private int fadeDuration;
        internal async Awaitable StartLoading()
        {
            root.SetActive(true);
            await group.Show(fadeDuration);
        }

        internal async Awaitable EndLoading()
        {
            await group.Hide(fadeDuration);
            root.SetActive(false);
        }
    }
}