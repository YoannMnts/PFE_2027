using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PFE.Core.Scripts.GameSettings
{
    public class GameSceneController
    {
        public SceneReference ActiveSceneReference { get; private set; }
        
        public SceneReference[] AdditionalSceneReferences { get; private set; }

        internal GameSceneController()
        {
            
        }

        public async Awaitable<Scene> LoadSceneWithLoadingScreen(SceneReference activeScene,
            params SceneReference[] additionalScenes)
        {
            await Awaitable.MainThreadAsync();
            return activeScene.LoadedScene;
        }
    }
}