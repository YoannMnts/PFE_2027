using Eflatun.SceneReference;
using Helteix.Tools.Phases;
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
            int[] additionalScenesIndices = new int[additionalScenes.Length];
            for (int i = 0; i < additionalScenes.Length; i++)
                additionalScenesIndices[i] = additionalScenes[i].BuildIndex;

            LoadingScenePhase loadingScenePhase = new(activeScene.BuildIndex, additionalScenesIndices);

            ActiveSceneReference = activeScene;
            AdditionalSceneReferences = additionalScenes;

            await loadingScenePhase.Run();

            return activeScene.LoadedScene;
        }
    }
}