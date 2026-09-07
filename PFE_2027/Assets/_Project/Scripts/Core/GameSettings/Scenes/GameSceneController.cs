using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PFE.Core.Scripts.GameSettings
{
    public class GameSceneController
    {
        /// <summary>
        /// Fades out and tears down the loading screen shown by the last LoadSceneWithLoadingScreen call.
        /// Call this once the loaded scene is actually ready to be revealed (e.g. once OnPhaseBegin
        /// listeners have finished spawning gameplay content), not immediately after loading finishes.
        /// </summary>
        public Awaitable HideLoadingScreen() => currentLoadingScreen.HideLoadingScreen();
        
        public SceneReference ActiveSceneReference { get; private set; }
        
        public SceneReference[] AdditionalSceneReferences { get; private set; }

        private LoadingScenePhase currentLoadingScreen;

        internal GameSceneController()
        {
            
        }

        public async Awaitable<Scene> LoadSceneWithLoadingScreen(SceneReference activeScene,
            params SceneReference[] additionalScenes)
        {
            int[] additionalScenesIndices = new int[additionalScenes.Length];
            for (int i = 0; i < additionalScenes.Length; i++)
                additionalScenesIndices[i] = additionalScenes[i].BuildIndex;

            LoadingScenePhase loadingScenePhase = new(activeScene.BuildIndex, additionalScenesIndices);
            currentLoadingScreen = loadingScenePhase;

            ActiveSceneReference = activeScene;
            AdditionalSceneReferences = additionalScenes;

            await loadingScenePhase.Run();

            return activeScene.LoadedScene;
        }
    }
}