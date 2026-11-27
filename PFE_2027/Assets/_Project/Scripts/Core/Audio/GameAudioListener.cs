using System;
using System.Diagnostics.CodeAnalysis;
using Helteix.Singletons.MonoSingletons;
using Helteix.Singletons.MonoSingletons.Attributes;
using UnityEngine;

namespace PFE.Core.Audio
{
    [DontDestroyOnLoad]
    [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeCameraMain")]
    public class GameAudioListener : MonoSingleton<GameAudioListener>
    {
        public event Action OnAudioUpdate;
        
        private Camera currentCam;

        protected override void OnAwake()
        {
            base.OnAwake();
            RefreshMainCamera();
            gameObject.AddComponent<AudioListener>();
        }

        private void LateUpdate()
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            RefreshMainCamera();

            if (currentCam)
            {
                transform.position = currentCam.transform.position;
                transform.rotation = currentCam.transform.rotation;
                Debug.Log($"{name} was updated at {transform.position} with {transform.rotation}");
            }
            else
            {
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
            }
            
            OnAudioUpdate?.Invoke();
        }

        private void RefreshMainCamera()
        {
            Camera main = Camera.main;
            
            if (main != currentCam)
            {
                currentCam = main;

                //PAS D'AUDIO LISTENER EN DOUBLE
                if (currentCam && currentCam.TryGetComponent(out AudioListener audioListener))
                    Destroy(audioListener);
            }
        }
    }
}