using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PFE.Core.Scripts.GameSettings
{
    public class LoadingScenePhase : Phase<IEnumerable<Scene>>
    {
        private readonly int[] scenesIndices;
        
        private Scene loadingScene;
        private SceneLoaderUI loaderUI;

        public LoadingScenePhase(int mainScene, params int[] additionalScenes)
        {
            scenesIndices = new int[additionalScenes.Length + 1];
            scenesIndices[0] = mainScene;

            for (int i = 0; i < additionalScenes.Length; i++)
                scenesIndices[i + 1] = additionalScenes[i];
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            loadingScene = SceneManager.CreateScene("LoadingScene");
            loaderUI = Object.Instantiate(GameSceneSettings.Current.LoaderPrefab, new InstantiateParameters()
            {
                scene = loadingScene,
            });

            await loaderUI.StartLoading();
            SceneManager.SetActiveScene(loadingScene);
        }

        protected override async Awaitable<IEnumerable<Scene>> Execute(CancellationToken token)
        {
            int loadedSceneCount = SceneManager.loadedSceneCount;
            Scene[] loadedScenes = new Scene[loadedSceneCount];
            for (int i = 0; i < loadedSceneCount; i++)
                loadedScenes[i] = SceneManager.GetSceneAt(i);

            for (int i = 0; i < loadedSceneCount; i++)
            {
                if(loadedScenes[i] != loadingScene)
                    await SceneManager.UnloadSceneAsync(loadedScenes[i]);
            }
            
            for (int i = 0; i < scenesIndices.Length; i++)
            {
                int sceneIndex = scenesIndices[i];
                await SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            }
            
            Scene newActiveScene = SceneManager.GetSceneByBuildIndex(scenesIndices[0]);
            SceneManager.SetActiveScene(newActiveScene);
            
            return scenesIndices.Select(SceneManager.GetSceneByBuildIndex);
        }

        protected override async Awaitable Dispose(CancellationToken token)
        {
            await loaderUI.EndLoading();
            await SceneManager.UnloadSceneAsync(loadingScene);
        }
    }
}